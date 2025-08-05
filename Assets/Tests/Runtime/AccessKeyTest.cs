/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using Internal;

    internal sealed class AccessKeyTest : MonoBehaviour {

        private void Start () => Debug.Log($"Access key: {MunaSettings.Instance.accessKey}");
    }
}