/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using System.Runtime.InteropServices;
    using UnityEngine;

    internal sealed class SystemInfoTest : MonoBehaviour {

        private void Start() {
            Debug.Log("Process arch: " + RuntimeInformation.ProcessArchitecture);
            Debug.Log("OS arch: " + RuntimeInformation.OSArchitecture);
            Debug.Log("macOS: " + RuntimeInformation.IsOSPlatform(OSPlatform.OSX));
            Debug.Log("Linux: " + RuntimeInformation.IsOSPlatform(OSPlatform.Linux));
            Debug.Log("Windows: " + RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        }
    }
}