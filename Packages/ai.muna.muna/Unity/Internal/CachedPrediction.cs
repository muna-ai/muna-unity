/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Internal {

    using System;

    /// <summary>
    /// Cached prediction.
    /// </summary>
    [Preserve, Serializable]
    internal class CachedPrediction : Prediction {

        public string target;

        [Preserve]
        #pragma warning disable CS8618
        public CachedPrediction() { }

        public CachedPrediction(Prediction prediction) {
            this.id = prediction.id;
            this.tag = prediction.tag;
            this.created = prediction.created;
            this.resources = prediction.resources;
            this.configuration = null;
        }
        #pragma warning restore CS8618
    }
}