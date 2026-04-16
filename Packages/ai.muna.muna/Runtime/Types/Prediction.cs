/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Muna {

    using System;
    using System.Runtime.Serialization;
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
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Acceleration : int {
        /// <summary>
        /// Automatically choose the best acceleration for the current device.
        /// </summary>
        [EnumMember(Value = @"local_auto")]
        LocalAuto,
        /// <summary>
        /// Predictions run on the CPU.
        /// </summary>
        [EnumMember(Value = @"local_cpu")]
        LocalCPU,
        /// <summary>
        /// Predictions run on the GPU.
        /// </summary>
        [EnumMember(Value = @"local_gpu")]
        LocalGPU,
        /// <summary>
        /// Predictions run on the neural processor.
        /// </summary>
        [EnumMember(Value = @"local_npu")]
        LocalNPU,
        /// <summary>
        /// Automatically choose the best acceleration.
        /// </summary>
        [EnumMember(Value = @"remote_auto")]
        RemoteAuto,
        /// <summary>
        /// Predictions run on a CPU.
        /// </summary>
        [EnumMember(Value = @"remote_cpu")]
        RemoteCPU,
        /// <summary>
        /// Predictions run on an Nvidia A10 GPU.
        /// </summary>
        [EnumMember(Value = @"remote_a10")]
        RemoteA10,
        /// <summary>
        /// Predictions run on an Nvidia L40S GPU.
        /// </summary>
        [EnumMember(Value = @"remote_l40s")]
        RemoteL40S,
        /// <summary>
        /// Predictions run on an Nvidia A100 GPU.
        /// </summary>
        [EnumMember(Value = @"remote_a100")]
        RemoteA100,
        /// <summary>
        /// Predictions run on an Nvidia H100 GPU.
        /// </summary>
        [EnumMember(Value = @"remote_h100")]
        RemoteH100,
        /// <summary>
        /// Predictions run on an Nvidia H200 GPU.
        /// </summary>
        [EnumMember(Value = @"remote_h200")]
        RemoteH200,
        /// <summary>
        /// Predictions run on an Nvidia B200 GPU.
        /// </summary>
        [EnumMember(Value = @"remote_b200")]
        RemoteB200,
        /// <summary>
        /// Predictions run on an AMD MI350X GPU.
        /// </summary>
        [EnumMember(Value = @"remote_mi350x")]
        RemoteMI350X,
        /// <summary>
        /// Predictions run on an AMD MI355X GPU.
        /// </summary>
        [EnumMember(Value = @"remote_mi355x")]
        RemoteMI355X,
        /// <summary>
        /// Predictions run on an Qualcomm Cloud AI 100.
        /// </summary>
        [EnumMember(Value = @"remote_qaic100")]
        RemoteQAIC100,
    }

    public static class AccelerationUtils {
        
        /// <summary>
        /// Convert an acceleration constant to a string.
        /// </summary>
        public static string ToAccelerationString(this Acceleration acc) => (string)acc.SerializeEnum()!;
    }
}