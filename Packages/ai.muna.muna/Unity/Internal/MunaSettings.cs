/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Internal {

    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Muna settings for the current Unity project.
    /// </summary>
    [DefaultExecutionOrder(int.MinValue)]
    internal sealed class MunaSettings : ScriptableObject {

        #region --Client API--
        /// <summary>
        /// Project-wide access key.
        /// </summary>
        [field: SerializeField, HideInInspector]
        public string accessKey { get; private set; } = string.Empty;

        /// <summary>
        /// Predictor cache.
        /// </summary>
        [field: SerializeField, HideInInspector]
        public List<CachedPrediction> cache { get; internal set; } = new();

        /// <summary>
        /// Settings instance for this project.
        /// </summary>
        #pragma warning disable 8618
        public static MunaSettings Instance;
        #pragma warning restore 8618

        /// <summary>
        /// Create Function settings.
        /// </summary>
        /// <param name="accessKey">Function access key.</param>
        public static MunaSettings Create(string accessKey) {
            var settings = CreateInstance<MunaSettings>();
            settings.accessKey = accessKey;
            return settings;
        }
        #endregion


        #region --Operations--

        private void Awake() {
            if (!Application.isEditor)
                Instance = this;
        }
        #endregion
    }
}