/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.API {

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using Internal;

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
            Dictionary<string, object?>? payload = default
        ) where T : class {
            // Check payload
            var tag = GetValue<string>(payload, @"tag");
            var target = GetValue<string>(payload, @"clientId");
            var configurationId = GetValue<string>(payload, @"configurationId");
            if (
                method != @"POST"                       ||
                path != @"/predictions"                 ||
                string.IsNullOrEmpty(tag)               ||
                string.IsNullOrEmpty(target)            ||
                string.IsNullOrEmpty(configurationId)
            )
                return await base.Request<T>(method, path, payload);
            // Get embedded prediction if available
            var cache = MunaSettings.Instance!.cache;
            var embeddedPrediction = cache.FirstOrDefault(p => 
                p.tag == tag &&
                ClientIdsCompatible(p.target!, target)
            );
            if (embeddedPrediction == null)
                return await base.Request<T>(method, path, payload);
            // Check for configuration token
            var configuration = await GetOrCreateConfigToken(
                embeddedPrediction,
                configurationId
            );
            var prediction = new Prediction {
                id = embeddedPrediction.id,
                tag = embeddedPrediction.tag,
                created = embeddedPrediction.created,
                resources = embeddedPrediction.resources,
                configuration = configuration,
            };
            // Return
            return prediction as T;
        }
        #endregion


        #region --Operations--

        private async Task<string> GetOrCreateConfigToken(
            CachedPrediction prediction,
            string configurationId
        ) {
            var key = $"{prediction.tag}.{prediction.target}.{configurationId}";
            if (PlayerPrefs.HasKey(key))
                return PlayerPrefs.GetString(key);
            var runtimePrediction = await base.Request<Prediction>(
                method: @"POST",
                path: @"/predictions",
                payload: new() {
                    [@"tag"] = prediction.tag,
                    [@"clientId"] = prediction.target,
                    [@"configurationId"] = configurationId,
                    [@"predictionId"] = prediction.id,
                }
            );
            var token = runtimePrediction!.configuration!;
            PlayerPrefs.SetString(key, token);
            PlayerPrefs.Save();
            return token;
        }

        private static bool ClientIdsCompatible(string a, string b) {
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

        private static T? GetValue<T>(
            Dictionary<string, object?>? payload,
            string key
        ) {
            if (payload?.TryGetValue(key, out var value) ?? false)
                return (T?)value;
            else
                return default;
        }
        #endregion
    }
}