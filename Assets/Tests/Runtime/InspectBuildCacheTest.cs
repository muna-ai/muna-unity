/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using Newtonsoft.Json;
    using Internal;

    internal sealed class InspectBuildCacheTest : MonoBehaviour {

        private void Start() {
            var settings = MunaSettings.Instance;
            Debug.Log(JsonConvert.SerializeObject(settings.cache, Formatting.Indented));
            Debug.Log(settings.accessKey);
        }
    }
}