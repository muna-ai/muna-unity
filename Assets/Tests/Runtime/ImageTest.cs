/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;

    internal sealed class ImageTest : MonoBehaviour {

        [Header(@"Image")]
        [SerializeField] private Texture2D image;

        [Header(@"UI")]
        [SerializeField] private UnityEngine.UI.RawImage rawImage;

        private async void Start() {
            // Predict
            var muna = MunaUnity.Create(url: @"https://api.fxn.dev");
            var prediction = await muna.Predictions.Create(
                tag: "@yusuf/image-identity",
                inputs: new() {
                    ["image"] = image.ToImage()
                }
            );
            // Display
            var result = (Image)prediction.results[0];
            rawImage.texture = result.ToTexture();
        }
    }
}