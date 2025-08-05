/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.C {

    using System;
    using static Function;

    public sealed class Predictor : IDisposable {

        #region --Client API--

        public Predictor (Configuration configuration) {
            CreatePredictor(configuration, out var predictor).Throw();
            this.predictor = predictor;
        }

        public Prediction CreatePrediction (ValueMap inputs) {
            predictor.CreatePrediction(inputs, out var prediction).Throw();
            return new Prediction(prediction);
        }

        public PredictionStream StreamPrediction (ValueMap inputs) {
            predictor.StreamPrediction(inputs, out var stream).Throw();
            return new PredictionStream(stream);
        }

        public void Dispose () => predictor.ReleasePredictor();
        #endregion


        #region --Operations--
        private readonly IntPtr predictor;

        public static implicit operator IntPtr (Predictor predictor) => predictor.predictor;
        #endregion
    }
}