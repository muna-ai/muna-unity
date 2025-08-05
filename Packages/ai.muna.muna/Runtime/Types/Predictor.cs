/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Function.Types {

    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Prediction function.
    /// </summary>
    [Preserve, Serializable]
    public class Predictor {

        /// <summary>
        /// Predictor tag.
        /// </summary>
        public string tag;

        /// <summary>
        /// Predictor owner.
        /// </summary>
        public User owner;

        /// <summary>
        /// Predictor name.
        /// </summary>
        public string name;

        /// <summary>
        /// Predictor description.
        /// </summary>
        public string description;
        
        /// <summary>
        /// Predictor status.
        /// </summary>
        public PredictorStatus status;

        /// <summary>
        /// Predictor access.
        /// </summary>
        public PredictorAccess access;

        /// <summary>
        /// Date created.
        /// </summary>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime created;

        /// <summary>
        /// Predictor card.
        /// </summary>
        public string? card;

        /// <summary>
        /// Predictor media URL.
        /// We encourage animated GIFs where possible.
        /// </summary>
        public string? media;

        /// <summary>
        /// Predictor signature.
        /// </summary>
        public Signature? signature;

        /// <summary>
        /// Predictor license URL.
        /// </summary>
        public string? license;
    }

    /// <summary>
    /// Predictor signature.
    /// </summary>
    [Preserve, Serializable]
    public class Signature {

        /// <summary>
        /// Prediction inputs.
        /// </summary>
        public Parameter[] inputs;

        /// <summary>
        /// Prediction outputs.
        /// </summary>
        public Parameter[] outputs;
    }

    /// <summary>
    /// Predictor parameter.
    /// </summary>
    [Preserve, Serializable]
    public class Parameter {

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string? name;

        /// <summary>
        /// Parameter type.
        /// </summary>
        public Dtype? type;

        /// <summary>
        /// Parameter description.
        /// </summary>
        public string? description;

        /// <summary>
        /// Parameter is optional.
        /// </summary>
        public bool? optional;

        /// <summary>
        /// Parameter value range for numeric parameters.
        /// </summary>
        public float[]? range;

        /// <summary>
        /// Parameter value choices for enumeration parameters.
        /// </summary>
        public EnumerationMember[]? enumeration;
    }

    /// <summary>
    /// Prediction parameter enumeration member.
    /// </summary>
    [Preserve, Serializable]
    public class EnumerationMember {
        /// <summary>
        /// Enumeration member name.
        /// </summary>
        public string name;
        /// <summary>
        /// Enumeration member value.
        /// This is usually a `string` or `int`.
        /// </summary>
        public object value;
    }

    /// <summary>
    /// Predictor access mode.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PredictorAccess : int {
        /// <summary>
        /// Predictor is public.
        /// </summary>
        [EnumMember(Value = @"public")]
        Public = 0,
        /// <summary>
        /// Predictor is private to the user or organization.
        /// </summary>
        [EnumMember(Value = @"private")]
        Private = 1,
        /// <summary>
        /// Predictor is public but unlisted from discovery.
        /// </summary>
        [EnumMember(Value = @"unlisted")]
        Unlisted = 2,
    }

    /// <summary>
    /// Predictor status.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PredictorStatus : int {
        /// <summary>
        /// Predictor is being compiled.
        /// </summary>
        [EnumMember(Value = @"compiling")]
        Compiling = 0,
        /// <summary>
        /// Predictor is active.
        /// </summary>
        [EnumMember(Value = @"active")]
        Active = 1,
        /// <summary>
        /// Predictor is archived.
        /// </summary>
        [EnumMember(Value = @"archived")]
        Archived = 3,
    }
}