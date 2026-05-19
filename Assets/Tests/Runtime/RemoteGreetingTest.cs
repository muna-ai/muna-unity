/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using Newtonsoft.Json;
    using Beta;

    internal sealed class RemoteGreetingTest : MonoBehaviour {

        public Acceleration acceleration;
        private const string Tag = "@fxn/greeting";

        private async void Start() {
            var muna = MunaUnity.Create();
            var prediction = await muna.Predictions.Create(
                tag: Tag,
                inputs: new() {
                    [@"name"] = "Yusuf"
                },
                acceleration: acceleration.AsString()
            );
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
        }
    }
}