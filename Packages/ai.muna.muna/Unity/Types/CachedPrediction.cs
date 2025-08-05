/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Types {

    using System;

    /// <summary>
    /// Cached prediction.
    /// </summary>
    [Preserve, Serializable]
    internal class CachedPrediction : Prediction {

        public string? clientId;

        [Preserve]
        public CachedPrediction () { }

        public CachedPrediction (Prediction prediction, string clientId) {
            this.id = prediction.id;
            this.tag = prediction.tag;
            this.created = prediction.created;
            this.results = prediction.results;
            this.latency = prediction.latency;
            this.error = prediction.error;
            this.logs = prediction.logs;
            this.resources = prediction.resources;
            this.configuration = prediction.configuration;
            this.clientId = clientId;
        }
    }
}