/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using System.Runtime.InteropServices;
    using UnityEngine;
    using C;

    internal sealed class FxncVersionTest : MonoBehaviour {

        private async void Start() {
            await Configuration.InitializationTask;
            Debug.Log("Initialized Muna!");
            // Version
            var version = Marshal.PtrToStringAuto(Muna.GetVersion());
            Debug.Log($"Muna {version}");
            Debug.Log($"Client: {Configuration.ClientId}");
            Debug.Log($"Configuration: {Configuration.ConfigurationId}");
        }
    }
}