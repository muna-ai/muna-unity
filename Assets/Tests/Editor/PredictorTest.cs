/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine.TestTools;

    internal sealed class PredictorTest {

        private Muna muna;

        [SetUp]
        public void Before () => muna = MunaUnity.Create();

        [Test(Description = @"Should retrieve a valid predictor")]
        public async Task RetrievePredictor() {
            var tag = "@fxn/greeting";
            var predictor = await muna.Predictors.Retrieve(tag);
            Assert.AreEqual(tag, predictor?.tag);
            Assert.AreEqual(PredictorStatus.Active, predictor?.status);
        }
    }
}