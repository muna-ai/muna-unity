/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Muna {

    using System;

    /// <summary>
    /// Predictor parameter.
    /// </summary>
    [Preserve, Serializable]
    public class Parameter {

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string name;

        /// <summary>
        /// Parameter type.
        /// </summary>
        public Dtype type;

        /// <summary>
        /// Parameter description.
        /// </summary>
        public string? description;

        /// <summary>
        /// Parameter denotation.
        /// </summary>
        public string? denotation;

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

        /// <summary>
        /// Parameter audio sample rate.
        /// </summary>
        public int? sampleRate;
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
}