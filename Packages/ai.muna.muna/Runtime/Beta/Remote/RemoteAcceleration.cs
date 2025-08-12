/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Beta {

    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Remote acceleration.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RemoteAcceleration : int {
        /// <summary>
        /// Automatically choose the best acceleration.
        /// </summary>
        [EnumMember(Value = @"remote_auto")]
        Auto = 0,
        /// <summary>
        /// Predictions run on a CPU.
        /// </summary>
        [EnumMember(Value = @"remote_cpu")]
        CPU = 1,
        /// <summary>
        /// Predictions run on an Nvidia A40 GPU.
        /// </summary>
        [EnumMember(Value = @"remote_a40")]
        A40 = 2,
        /// <summary>
        /// Predictions run on an Nvidia A100 GPU.
        /// </summary>
        [EnumMember(Value = @"remote_a100")]
        A100 = 3,
    }
}