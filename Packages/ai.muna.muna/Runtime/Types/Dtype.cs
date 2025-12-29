/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

namespace Muna {

    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Value type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Dtype : int { // CHECK // Must match `Function.h`
        /// <summary>
        /// Null or unsupported data type.
        /// </summary>
        [EnumMember(Value = @"null")]
        Null = 0,
        /// <summary>
        /// IEEE 754 half precision 16-bit float.
        /// </summary>
        [EnumMember(Value = @"float16")]
        Float16 = 1,
        /// <summary>
        /// IEEE 754 single precision 32-bit float.
        /// </summary>
        [EnumMember(Value = @"float32")]
        Float32 = 2,
        /// <summary>
        /// IEEE 754 double precision 64-bit float.
        /// </summary>
        [EnumMember(Value = @"float64")]
        Float64 = 3,
        /// <summary>
        /// Signed 8-bit integer.
        /// </summary>
        [EnumMember(Value = @"int8")]
        Int8 = 4,
        /// <summary>
        /// Signed 16-bit integer.
        /// </summary>
        [EnumMember(Value = @"int16")]
        Int16 = 5,
        /// <summary>
        /// Signed 32-bit integer.
        /// </summary>
        [EnumMember(Value = @"int32")]
        Int32 = 6,
        /// <summary>
        /// Signed 64-bit integer.
        /// </summary>
        [EnumMember(Value = @"int64")]
        Int64 = 7,
        /// <summary>
        /// Unsigned 8-bit integer.
        /// </summary>
        [EnumMember(Value = @"uint8")]
        Uint8 = 8,
        /// <summary>
        /// Unsigned 16-bit integer.
        /// </summary>
        [EnumMember(Value = @"uint16")]
        Uint16 = 9,
        /// <summary>
        /// Unsigned 32-bit integer.
        /// </summary>
        [EnumMember(Value = @"uint32")]
        Uint32 = 10,
        /// <summary>
        /// Unsigned 64-bit integer.
        /// </summary>
        [EnumMember(Value = @"uint64")]
        Uint64 = 11,
        /// <summary>
        /// 8-bit boolean where zero is `false` and non-zero is `true`.
        /// </summary>
        [EnumMember(Value = @"bool")]
        Bool = 12,
        /// <summary>
        /// UTF-8 encoded string.
        /// </summary>
        [EnumMember(Value = @"string")]
        String = 13,
        /// <summary>
        /// JSON-serializable list.
        /// </summary>
        [EnumMember(Value = @"list")]
        List = 14,
        /// <summary>
        /// JSON-serializable dictionary.
        /// </summary>
        [EnumMember(Value = @"dict")]
        Dict = 15,
        /// <summary>
        /// Pixel buffer with 8 bits per intensity, interleaved by channel.
        /// </summary>
        [EnumMember(Value = @"image")]
        Image = 16,
        /// <summary>
        /// Binary blob.
        /// </summary>
        [EnumMember(Value = @"binary")]
        Binary = 17,
        /// <summary>
        /// 16-bit brain float.
        /// </summary>
        [EnumMember(Value = @"bfloat16")]
        BFloat16 = 18,
        /// <summary>
        /// Prediction value list.
        /// </summary>
        [EnumMember(Value = @"value_list")]
        ValueList = 19,
        /// <summary>
        /// Prediction value map.
        /// </summary>
        [EnumMember(Value = @"value_map")]
        ValueMap = 20,
    }
}