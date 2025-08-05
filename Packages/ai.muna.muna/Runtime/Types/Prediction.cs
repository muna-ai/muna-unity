/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Function.Types {

    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Prediction.
    /// </summary>
    [Preserve, Serializable]
    public class Prediction {

        /// <summary>
        /// Prediction ID.
        /// </summary>
        public string id;

        /// <summary>
        /// Predictor tag.
        /// </summary>
        public string tag;

        /// <summary>
        /// Date created.
        /// </summary>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime created;

        /// <summary>
        /// Prediction results.
        /// </summary>
        public object?[]? results;

        /// <summary>
        /// Prediction latency in milliseconds.
        /// </summary>
        public double? latency;

        /// <summary>
        /// Prediction error.
        /// This is `null` if the prediction completed successfully.
        /// </summary>
        public string? error;

        /// <summary>
        /// Prediction logs.
        /// </summary>
        public string? logs;

        /// <summary>
        /// Predictor resources.
        /// </summary>
        public PredictionResource[]? resources;

        /// <summary>
        /// Prediction configuration token.
        /// </summary>
        public string? configuration;
    }

    /// <summary>
    /// Prediction resource.
    /// </summary>
    [Preserve, Serializable]
    public class PredictionResource {

        /// <summary>
        /// Resource type.
        /// </summary>
        public string type;

        /// <summary>
        /// Resource URL.
        /// </summary>
        public string url;

        /// <summary>
        /// Resource name.
        /// </summary>
        public string? name;
    }

    /// <summary>
    /// Prediction acceleration.
    /// </summary>
    [Flags]
    public enum Acceleration : int {
        /// <summary>
        /// Automatically choose the best acceleration for the current device.
        /// </summary>
        Auto = 0,
        /// <summary>
        /// Predictions run on the CPU.
        /// </summary>
        CPU = 1 << 0,
        /// <summary>
        /// Predictions run on the GPU.
        /// </summary>
        GPU = 1 << 1,
        /// <summary>
        /// Predictions run on the neural processor.
        /// </summary>
        NPU = 1 << 2,
    }
}