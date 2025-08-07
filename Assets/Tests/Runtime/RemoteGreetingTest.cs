/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using Newtonsoft.Json;
    using Beta;

    internal sealed class RemoteGreetingTest : MonoBehaviour {

        public RemoteAcceleration acceleration;
        private const string Tag = "@fxn/greeting";

        private async void Start() {
            var muna = MunaUnity.Create();
            var prediction = await muna.Beta.Predictions.Remote.Create(
                tag: Tag,
                inputs: new() {
                    [@"name"] = "Yusuf"
                },
                acceleration: acceleration
            );
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
        }
    }
}