/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.C {

    using System;
    using System.Text;
    using static Function;

    public sealed class Prediction : IDisposable {

        #region --Client API--

        public string id {
            get {
                var id = new StringBuilder(64);
                prediction.GetPredictionID(id, id.Capacity).Throw();
                return id.ToString();
            }
        }

        public double latency {
            get {
                prediction.GetPredictionLatency(out var latency).Throw();
                return latency;
            }
        }

        public ValueMap? results {
            get {
                var map = prediction.GetPredictionResults(out var m).Throw() == Status.Ok ?
                    new ValueMap(m) :
                    default;
                return map?.size > 0 ? map : default;
            }
        }

        public string? error {
            get {
                var error = new StringBuilder(2048);
                return prediction.GetPredictionError(error, error.Capacity) == Status.Ok ? error.ToString() : null;
            }
        }

        public string logs {
            get {
                prediction.GetPredictionLogLength(out var length).Throw();
                var logs = new StringBuilder(length + 1);
                prediction.GetPredictionLogs(logs, logs.Capacity);
                return logs.ToString();
            }
        }

        public void Dispose () => prediction.ReleasePrediction();
        #endregion


        #region --Operations--
        private readonly IntPtr prediction;

        internal Prediction (IntPtr prediction) => this.prediction = prediction;

        public static implicit operator IntPtr (Prediction prediction) => prediction.prediction;
        #endregion
    }
}