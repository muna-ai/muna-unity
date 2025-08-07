/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using System.Threading.Tasks;
    using NUnit.Framework;

    internal sealed class PredictionTest {

        private Muna muna;

        [SetUp]
        public void Before() => muna = MunaUnity.Create();

        [Test(Description = @"Should create a prediction")]
        public async Task CreatePrediction() {
            var prediction = await muna.Predictions.Create(
                tag: "@yusuf/area",
                inputs: new () {
                    ["radius"] = 3f,
                }
            );
            var result = prediction?.results?[0];
            Assert.NotNull(result);
        }
    }
}