/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Internal {

    using System.Collections.Generic;
    using UnityEngine;
    using Types;

    /// <summary>
    /// Function settings for the current Unity project.
    /// </summary>
    [DefaultExecutionOrder(int.MinValue)]
    internal sealed class FunctionSettings : ScriptableObject {

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
        public static FunctionSettings Instance;
        #pragma warning restore 8618

        /// <summary>
        /// Create Function settings.
        /// </summary>
        /// <param name="accessKey">Function access key.</param>
        public static FunctionSettings Create (string accessKey) {
            var settings = CreateInstance<FunctionSettings>();
            settings.accessKey = accessKey;
            return settings;
        }
        #endregion


        #region --Operations--

        private void Awake () {
            // Set singleton in player
            if (!Application.isEditor)
                Instance = this;
        }
        #endregion
    }
}