/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using Newtonsoft.Json;
    using Internal;

    internal sealed class StreamingTest : MonoBehaviour {

        private async void Start() {
            var fxn = MunaUnity.Create(
                accessKey: MunaSettings.Instance.accessKey,
                url: @"https://api.fxn.dev"
            );
            var stream = fxn.Predictions.Stream(
                tag: "@yusuf-delete/streaming",
                inputs: new () {
                    ["sentence"] = @"Hello world"
                }
            );
            await foreach (var prediction in stream)
                Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
            Debug.Log("Done!");
        }
    }
}