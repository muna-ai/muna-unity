/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Internal {

    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using UnityEngine;
    using Newtonsoft.Json;
    using Services;

    internal static class PredictionCache {

        #region --Types--
        /// <summary>
        /// Cached prediction.
        /// </summary>
        [Preserve, Serializable]
        internal class CachedPrediction : Prediction {
            public string? clientId;
            public string? configurationId;

            [Preserve]
            public CachedPrediction() { }
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// Add a prediction to the cache.
        /// </summary>
        public static void Add(CachedPrediction prediction) {
            var path = GetCachedPath(prediction);
            var json = JsonConvert.SerializeObject(prediction, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Get a prediction from the cache.
        /// </summary>
        public static bool Get(
            string tag,
            string clientId,
            string configurationId,
            PredictionResource[]? embeddedResources,
            out CachedPrediction? prediction
        ) {
            prediction = null;
            // Check filesystem
            var path = GetCachedPath(tag, clientId, configurationId);
            if (!File.Exists(path))
                return false;
            // Load prediction
            var json = File.ReadAllText(path);
            var result = JsonConvert.DeserializeObject<CachedPrediction>(json)!;
            // Check that resources are valid
            if (!ResourcesAreValid(result.resources!, embeddedResources)) {
                File.Delete(path);
                return false;
            }
            // Pass
            prediction = result;
            return true;
        }

        /// <summary>
        /// Remove a prediction from the cache.
        /// </summary>
        /// <param name="prediction"></param>
        public static void Remove(CachedPrediction prediction) {
            var path = GetCachedPath(prediction);
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// Create a cached prediction from a prediction.
        /// </summary>
        public static CachedPrediction AsCached(
            this Prediction prediction,
            string? clientId = null,
            string? configurationId = null
        ) => new() {
            id = prediction.id,
            tag = prediction.tag,
            created = prediction.created,
            results = prediction.results,
            latency = prediction.latency,
            error = prediction.error,
            logs = prediction.logs,
            resources = prediction.resources,
            configuration = prediction.configuration,
            clientId = clientId,
            configurationId = configurationId
        };
        #endregion


        #region --Operations--
        private static string CacheRoot => Application.isEditor ?
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @".fxn") :
            Path.Combine(Application.persistentDataPath, @"fxn");
        internal static string ResourceCachePath => Path.Combine(CacheRoot, @"cache");
        internal static string PredictorCachePath => Path.Combine(CacheRoot, @"predictors");

        private static string GetCachedPath(CachedPrediction prediction) => GetCachedPath(
            tag: prediction.tag,
            clientId: prediction.clientId!,
            configurationId: prediction.configurationId!
        );

        private static string GetCachedPath(
            string tag,
            string clientId,
            string configurationId
        ) {
            if (!Directory.Exists(PredictorCachePath))
                Directory.CreateDirectory(PredictorCachePath);
            using var sha256 = new SHA256Managed();
            var identifier = $"{tag}::{clientId}::{configurationId}";
            var identifierData = Encoding.UTF8.GetBytes(identifier);
            var hashData = sha256.ComputeHash(identifierData);
            var hash = BitConverter.ToString(hashData).Replace("-", "").ToLower();
            var path = Path.Combine(PredictorCachePath, $"{hash}.json");
            return path;
        }

        private static bool ResourcesAreValid(
            PredictionResource[] cachedResources,
            PredictionResource[]? embeddedResources
        ) {
            if (embeddedResources != null && cachedResources.Length != embeddedResources.Length)
                return false;
            for (var idx = 0; idx < cachedResources.Length; ++idx) {
                var cachedResource = cachedResources[idx];
                var cachedPath = new Uri(cachedResource.url).LocalPath;
                var embeddedPath = embeddedResources != null ?
                    PredictionService.GetResourcePath(embeddedResources[idx], ResourceCachePath) :
                    null;
                if (!File.Exists(cachedPath))
                    return false;
                if (embeddedPath != null && cachedPath != embeddedPath)
                    return false;
            }
            return true;
        }
        #endregion
    }
}