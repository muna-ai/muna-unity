/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta {

    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Remote prediction value.
    /// </summary>
    [Preserve, Serializable]
    public sealed class RemoteValue {

        /// <summary>
        /// Value URL.
        /// </summary>
        public string? data;

        /// <summary>
        /// Value type.
        /// </summary>
        public Dtype dtype;

        /// <summary>
        /// Value shape.
        /// This is `null` if shape information is not available or applicable.
        /// </summary>
        public int[]? shape;
    }
}