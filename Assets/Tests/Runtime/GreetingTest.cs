/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using Newtonsoft.Json;

    [Muna.Embed(Tag)]
    internal sealed class GreetingTest : MonoBehaviour {

        private const string Tag = "@fxn/greeting";

        private async void Start() {
            // Predict
            var muna = MunaUnity.Create();
            var prediction = await muna.Predictions.Create(
                tag: Tag,
                inputs: new() {
                    [@"name"] = "Yusuf"
                }
            );
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
            // Unload from memory
            var deleted = await muna.Predictions.Delete(Tag);
            Debug.Log($"Deleted predictor: {deleted}");
        }
    }
}