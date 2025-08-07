/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.C {

    using System;
    using static Muna;

    public sealed class PredictionStream : IDisposable {

        #region --Client API--

        public Prediction? ReadNext() {
            if (stream.ReadNextPrediction(out var prediction) == Status.Ok)
                return new Prediction(prediction);
            else
                return null;
        }

        public void Dispose() => stream.ReleasePredictionStream();
        #endregion


        #region --Operations--
        private readonly IntPtr stream;

        internal PredictionStream(IntPtr stream) => this.stream = stream;

        public static implicit operator IntPtr(PredictionStream stream) => stream.stream;
        #endregion
    }
}