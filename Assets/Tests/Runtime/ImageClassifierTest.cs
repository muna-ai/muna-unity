/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using Newtonsoft.Json;

    [Muna.Embed(Tag)]
    internal sealed class ImageClassifierTest : MonoBehaviour {

        [Header(@"Image")]
        [SerializeField] private Texture2D image;

        private const string Tag = "@yusuf/mobilenet-v2";

        private async void Start() {
            var muna = MunaUnity.Create();
            var prediction = await muna.Predictions.Create(
                tag: Tag,
                inputs: new() {
                    ["image"] = image.ToImage()
                }
            );
            Debug.Log(JsonConvert.SerializeObject(prediction, formatting: Formatting.Indented));
        }
    }
}