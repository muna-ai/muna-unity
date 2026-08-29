/*
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Configuration = C.Configuration;

    /// <summary>
    /// Prediction cache.
    /// This caches predictions used to provision local predictors, along with
    /// their configuration tokens and resources. Configuration tokens are
    /// refreshed once they fall within half of their lifetime, providing the
    /// liveness signal used to count monthly active runtimes.
    /// </summary>
    internal class PredictionCache {

        #region --Client API--
        /// <summary>
        /// Create the prediction cache.
        /// </summary>
        /// <param name="client">Muna API client.</param>
        /// <param name="now">Clock returning the current Unix timestamp in seconds. Used for testing.</param>
        public PredictionCache(
            MunaClient client,
            Func<long>? now = default
        ) {
            this.client = client;
            this.now = now ?? (() => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        /// <summary>
        /// Pin the prediction that is requested from the API for a given predictor.
        /// When retrieving a prediction that is not yet cached, the fetch is pinned
        /// to the given prediction identifier, ensuring that the returned
        /// configuration token is signed with the key corresponding to the
        /// predictor implementation embedded at build time.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="target">Prediction target.</param>
        /// <param name="predictionId">Prediction identifier to pin fetches to.</param>
        public void Pin(
            string tag,
            string target,
            string predictionId
        ) => seeds.Add((tag, target, predictionId));

        /// <summary>
        /// Retrieve a cached prediction.
        /// This serves the cached prediction when its configuration token is
        /// within half of its lifetime, and fetches a prediction from the API
        /// otherwise, falling back to the cached prediction when the API is
        /// unreachable. Prediction resources in the returned prediction always
        /// point to local files.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <returns>Cached prediction.</returns>
        public async Task<Prediction> Retrieve(string tag) {
            var (clientId, configurationId) = await ResolveIdentity();
            var key = GetCacheKey(tag, clientId, configurationId);
            var prediction = LoadPrediction(key);
            if (
                !string.IsNullOrEmpty(prediction?.configuration) &&
                !ShouldRefreshToken(prediction!.configuration!, now())
            )
                return await Localize(prediction!);
            try {
                var refreshed = await FetchPrediction(
                    tag,
                    clientId,
                    configurationId,
                    key,
                    cachedPredictionId: prediction?.id
                );
                return await Localize(refreshed);
            } catch when (prediction != null) {
                // Configuration token expiry is a refresh hint. A failed refresh
                // must not prevent an already-provisioned device from working offline.
                return await Localize(prediction!);
            }
        }

        /// <summary>
        /// Invalidate a cached prediction.
        /// The evicted prediction identifier is remembered so that the next
        /// retrieval is pinned to it, ensuring that the fetched configuration
        /// token corresponds to the same predictor implementation.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        public async Task Invalidate(string tag) {
            var (clientId, configurationId) = await ResolveIdentity();
            var key = GetCacheKey(tag, clientId, configurationId);
            var prediction = LoadPrediction(key);
            if (!string.IsNullOrEmpty(prediction?.id))
                invalidatedPredictionIds[key] = prediction!.id;
            client.SetCacheEntry(key, null);
        }
        #endregion


        #region --Operations--
        private readonly MunaClient client;
        private readonly Func<long> now;
        private (string clientId, string configurationId)? identity;
        private readonly List<(string tag, string target, string predictionId)> seeds = new();
        private readonly Dictionary<string, string> invalidatedPredictionIds = new();

        private async Task<(string clientId, string configurationId)> ResolveIdentity() {
            if (identity != null)
                return identity.Value;
            await Configuration.InitializationTask;
            identity = (
                Configuration.ClientId,
                Configuration.ConfigurationId
            );
            return identity.Value;
        }

        private async Task<Prediction> FetchPrediction(
            string tag,
            string clientId,
            string configurationId,
            string key,
            string? cachedPredictionId
        ) {
            invalidatedPredictionIds.TryGetValue(key, out var invalidatedPredictionId);
            var predictionId =
                invalidatedPredictionId ??
                cachedPredictionId ??
                FindPinnedPredictionId(tag, clientId);
            var payload = new Dictionary<string, object?> {
                [@"tag"] = tag,
                [@"clientId"] = clientId,
                [@"configurationId"] = configurationId,
            };
            if (!string.IsNullOrEmpty(predictionId))
                payload[@"predictionId"] = predictionId;
            var prediction = await client.Request<Prediction>(
                method: @"POST",
                path: @"/predictions",
                payload: payload
            );
            client.SetCacheEntry(key, JsonConvert.SerializeObject(prediction));
            invalidatedPredictionIds.Remove(key);
            return prediction!;
        }

        private Prediction? LoadPrediction(string key) {
            var json = client.GetCacheEntry(key);
            if (string.IsNullOrEmpty(json))
                return null;
            try {
                return JsonConvert.DeserializeObject<Prediction>(json!);
            } catch {
                client.SetCacheEntry(key, null);
                return null;
            }
        }

        private string? FindPinnedPredictionId(
            string tag,
            string clientId
        ) => seeds
            .Where(seed => seed.tag == tag && ClientIdsCompatible(seed.target, clientId))
            .Select(seed => seed.predictionId)
            .FirstOrDefault();

        private async Task<Prediction> Localize(Prediction prediction) {
            var resources = await Task.WhenAll(
                (prediction.resources ?? Array.Empty<PredictionResource>()).Select(DownloadResource)
            );
            return new Prediction {
                id = prediction.id,
                tag = prediction.tag,
                created = prediction.created,
                resources = resources,
                configuration = prediction.configuration,
            };
        }

        private async Task<PredictionResource> DownloadResource(PredictionResource resource) {
            var uri = new Uri(resource.url);
            if (uri.IsFile)
                return new() {
                    type = resource.type,
                    url = uri.LocalPath,
                    name = resource.name
                };
            var resourceDir = Path.Combine(client.cachePath, @"cache");
            var path = GetResourcePath(resource, resourceDir);
            if (!File.Exists(path)) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var dataStream = await client.Download(resource.url);
                using var fileStream = File.Create(path);
                dataStream.CopyTo(fileStream);
            }
            return new() {
                type = resource.type,
                url = path,
                name = resource.name
            };
        }

        internal static bool ShouldRefreshToken(
            string token,
            long now
        ) {
            try {
                var parts = token.Split('.');
                if (parts.Length < 2)
                    return true;
                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                var claims = JObject.Parse(json);
                var exp = claims.Value<long?>(@"exp");
                if (exp == null)
                    return false; // Legacy tokens do not expire.
                var iat = claims.Value<long?>(@"iat");
                if (iat == null || exp <= iat)
                    return now >= exp;
                var lifetime = exp.Value - iat.Value;
                var refreshAt = iat.Value + lifetime / 2L;
                return now >= refreshAt;
            } catch {
                // Revalidate malformed cache entries, while still allowing stale
                // fallback if the refresh cannot reach the API.
                return true;
            }
        }

        internal static string GetCacheKey(
            string tag,
            string clientId,
            string configurationId
        ) => $"muna.prediction.{EncodeKey(tag, clientId, configurationId)}";

        internal static string GetResourcePath(
            PredictionResource resource,
            string cacheDir
        ) {
            var uri = new Uri(resource.url);
            var stem = Path.GetFileName(uri.AbsolutePath);
            var path = string.IsNullOrEmpty(resource.name) ?
                Path.Combine(cacheDir, stem) :
                Path.Combine(cacheDir, stem, resource.name);
            return path;
        }

        private static string EncodeKey(params string[] values) => string.Concat(
            values.Select(value => $"{value.Length}:{value}")
        );

        private static bool ClientIdsCompatible(string a, string b) {
            if (a == b)
                return true;
            if (a.Contains(@"android") && b.Contains(@"android")) {
                var ARM32 = new[] { @"armeabi-v7a", @"armv7l", @"armv8l" };
                var ARM64 = new[] { @"arm64", @"aarch64", @"armv8" };
                if (ARM32.Any(s => a.Contains(s)) && ARM32.Any(s => b.Contains(s)))
                    return true;
                if (ARM64.Any(s => a.Contains(s)) && ARM64.Any(s => b.Contains(s)))
                    return true;
            }
            return false;
        }
        #endregion
    }
}
