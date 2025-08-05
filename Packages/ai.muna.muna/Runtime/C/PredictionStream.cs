/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.C {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using static Function;

    public sealed class PredictionStream : IEnumerable<Prediction>, IDisposable {

        #region --Client API--

        public void Dispose () => stream.ReleasePredictionStream();
        #endregion


        #region --Operations--
        private readonly IntPtr stream;

        internal PredictionStream (IntPtr stream) => this.stream = stream;

        IEnumerator<Prediction> IEnumerable<Prediction>.GetEnumerator () {
            while (true) {
                if (stream.ReadNextPrediction(out var prediction) == Status.Ok)
                    yield return new Prediction(prediction);
                else
                    yield break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator () => (this as IEnumerable<Prediction>).GetEnumerator();

        public static implicit operator IntPtr (PredictionStream stream) => stream.stream;
        #endregion
    }
}