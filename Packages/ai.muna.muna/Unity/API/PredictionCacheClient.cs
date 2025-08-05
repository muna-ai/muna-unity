/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using Newtonsoft.Json;
    using Services;
    using Types;

    /// <summary>
    /// Function API client for Unity Engine.
    /// This uses Unity APIs for performing web requests.
    /// Furthermore, this handles partial prediction caching for edge predictors.
    /// </summary>
    internal sealed class PredictionCacheClient : UnityClient {

        #region --Client API--
        /// <summary>
        /// Create the client.
        /// </summary>
        /// <param name="url">Function API URL.</param>
        /// <param name="accessKey">Function access key.</param>
        /// <param name="cache">Prediction cache.</param>
        public PredictionCacheClient (
            string url,
            string? accessKey,
            List<CachedPrediction>? cache = default
        ) : base(url, accessKey) => this.cache = cache ?? new();

        /// <summary>
        /// Perform a request to a Function REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <param name="headers">Request body.</param>
        /// <returns>Deserialized response.</returns>
        public override async Task<T?> Request<T> (
            string method,
            string path,
            Dictionary<string, object?>? payload = default,
            Dictionary<string, string>? headers = default
        ) where T : class {
            // Check payload
            var tag = payload?.TryGetValue(@"tag", out var t) ?? false ? t as string : null;
            var clientId = payload?.TryGetValue(@"clientId", out var id) ?? false ? id as string : null;
            var configurationId = payload?.TryGetValue(@"configurationId", out var configuration) ?? false ?
                configuration as string :
                null;
            if (
                method != @"POST"                       ||
                path != @"/predictions"                 ||
                string.IsNullOrEmpty(tag)               ||
                string.IsNullOrEmpty(clientId)          ||
                string.IsNullOrEmpty(configurationId)
            )
                return await base.Request<T>(method, path, payload, headers);
            // Get cached prediction
            var sanitizedTag = tag.Substring(1).Replace("/", "_");
            var cachePath = Path.Combine(
                PredictorCachePath,
                clientId,
                configurationId,
                $"{sanitizedTag}.json"
            );
            var cachedPrediction = TryLoadCachedPrediction(cachePath);
            if (cachedPrediction != null)
                return cachedPrediction as T;
            // Create prediction
            var predictionId = cache.FirstOrDefault(p => p.tag == tag && p.clientId == clientId)?.id;
            var prediction = await base.Request<Prediction>(
                method: @"POST",
                path: @"/predictions",
                payload: new () {
                    [@"tag"] = tag,
                    [@"clientId"] = clientId,
                    [@"configurationId"] = configurationId,
                    [@"predictionId"] = predictionId,
                },
                headers
            );
            prediction!.resources = await Task.WhenAll(prediction.resources.Select(GetCachedResource));
            // Write
            var predictionJson = JsonConvert.SerializeObject(
                new CachedPrediction(prediction, clientId),
                Formatting.Indented
            );
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
            File.WriteAllText(cachePath, predictionJson);
            // Return
            return prediction as T;
        }
        #endregion


        #region --Operations--
        private readonly List<CachedPrediction> cache;
        private static string CacheRoot => Application.isEditor ?
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @".fxn") :
            Path.Combine(Application.persistentDataPath, @"fxn");
        private static string ResourceCachePath => Path.Combine(CacheRoot, @"cache");
        private static string PredictorCachePath => Path.Combine(CacheRoot, @"predictors");

        private async Task<PredictionResource> GetCachedResource (PredictionResource resource) {
            var path = PredictionService.GetResourcePath(resource, ResourceCachePath);
            if (!File.Exists(path)) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var dataStream = await Download(resource.url);
                using var fileStream = File.Create(path);
                dataStream.CopyTo(fileStream);
            }
            return new PredictionResource { type = resource.type, url = $"file://{path}" };
        }

        private static Prediction? TryLoadCachedPrediction (string path) {
            if (!File.Exists(path))
                return null;
            var json = File.ReadAllText(path);
            var prediction = JsonConvert.DeserializeObject<Prediction>(json)!;
            var resources = prediction.resources.Select(res => new PredictionResource {
                type = res.type,
                url = $"file://{PredictionService.GetResourcePath(res, ResourceCachePath)}",
                name = res.name
            }).ToArray();
            if (!resources.All(res => File.Exists(new Uri(res.url).LocalPath))) {
                File.Delete(path);
                return null;
            }
            prediction.resources = resources;
            return prediction;
        }
        #endregion
    }
}