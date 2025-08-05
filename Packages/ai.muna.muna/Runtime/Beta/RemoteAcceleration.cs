/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Beta {

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
        [EnumMember(Value = @"auto")]
        Auto = 0,
        /// <summary>
        /// Predictions run on a CPU.
        /// </summary>
        [EnumMember(Value = @"cpu")]
        CPU = 1,
        /// <summary>
        /// Predictions run on an Nvidia A40 GPU.
        /// </summary>
        [EnumMember(Value = @"a40")]
        A40 = 2,
        /// <summary>
        /// Predictions run on an Nvidia A100 GPU.
        /// </summary>
        [EnumMember(Value = @"a100")]
        A100 = 3,
    }
}