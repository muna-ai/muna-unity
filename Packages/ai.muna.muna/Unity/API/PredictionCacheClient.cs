/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.API {

    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using Internal;
    using Services;

    /// <summary>
    /// Muna API client for Unity Engine.
    /// This uses Unity APIs for performing web requests.
    /// Furthermore, this handles partial prediction caching for edge predictors.
    /// </summary>
    internal sealed class PredictionCacheClient : UnityClient {

        #region --Client API--
        /// <summary>
        /// Create the client.
        /// </summary>
        /// <param name="url">Muna API URL.</param>
        /// <param name="accessKey">Muna access key.</param>
        /// <param name="cache">Prediction cache.</param>
        public PredictionCacheClient(
            string url,
            string? accessKey
        ) : base(url, accessKey) { }

        /// <summary>
        /// Perform a request to a Muna REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <param name="headers">Request body.</param>
        /// <returns>Deserialized response.</returns>
        public override async Task<T?> Request<T>(
            string method,
            string path,
            Dictionary<string, object?>? payload = default,
            Dictionary<string, string>? headers = default
        ) where T : class {
            // Check payload
            var tag = GetValue<string>(payload, @"tag");
            var clientId = GetValue<string>(payload, @"clientId");
            var configurationId = GetValue<string>(payload, @"configurationId");
            if (
                method != @"POST"                       ||
                path != @"/predictions"                 ||
                string.IsNullOrEmpty(tag)               ||
                string.IsNullOrEmpty(clientId)          ||
                string.IsNullOrEmpty(configurationId)
            )
                return await base.Request<T>(method, path, payload, headers);
            // Get embedded prediction if available
            var cache = MunaSettings.Instance!.cache;
            var embeddedPrediction = cache.FirstOrDefault(p => 
                p.tag == tag &&
                MatchClientIds(p.clientId!, clientId)
            );
            // Load from prediction cache
            if (PredictionCache.Get(
                tag,
                clientId,
                configurationId,
                embeddedPrediction?.resources,
                out var cachedPrediction
            ))
                return cachedPrediction as T;
            // Create prediction
            var prediction = await base.Request<Prediction>(
                method: @"POST",
                path: @"/predictions",
                payload: new() {
                    [@"tag"] = tag,
                    [@"clientId"] = clientId,
                    [@"configurationId"] = configurationId,
                    [@"predictionId"] = embeddedPrediction?.id,
                },
                headers
            );
            // Download resources
            var resources = new PredictionResource[prediction!.resources.Length];
            for (var i = 0; i < resources.Length; ++i)
                resources[i] = await GetCachedResource(prediction.resources[i]);
            prediction.resources = resources;
            // Cache
            PredictionCache.Add(prediction.AsCached(clientId, configurationId));
            // Return
            return prediction as T;
        }
        #endregion


        #region --Operations--

        private async Task<PredictionResource> GetCachedResource(PredictionResource resource) {
            var path = PredictionService.GetResourcePath(resource, PredictionCache.ResourceCachePath);
            if (!File.Exists(path)) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var dataStream = await Download(resource.url);
                using var fileStream = File.Create(path);
                dataStream.CopyTo(fileStream);
            }
            return new PredictionResource { type = resource.type, url = $"file://{path}" };
        }

        private static T? GetValue<T>(
            Dictionary<string, object?>? payload,
            string key
        ) {
            if (payload?.TryGetValue(key, out var value) ?? false)
                return (T?)value;
            else
                return default;
        }

        private static bool MatchClientIds(string a, string b) {
            if (a == b)
                return true;
            if (a.Contains("android") && b.Contains("android")) {
                var ARM32 = new[] { "armeabi-v7a", "armv7l", "armv8l" };
                var ARM64 = new[] { "arm64", "aarch64", "armv8" };
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