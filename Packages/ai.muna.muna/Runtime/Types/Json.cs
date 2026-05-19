/*
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna {

    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// JSON value.
    /// </summary>
    [Preserve]
    public readonly struct Json {

        #region --Client API--
        /// <summary>
        /// Create the JSON object from UTF-8 encoded data.
        /// </summary>
        public Json(byte[] data) => this.data = data;

        /// <summary>
        /// Create the JSON object from a string.
        /// </summary>
        public Json(string data) => this.data = Encoding.UTF8.GetBytes(data);

        /// <summary>
        /// Deserialize the JSON object.
        /// </summary>
        public T? ToObject<T>() {
            if (data == null)
                return default;
            using var reader = CreateReader();
            return JsonSerializer.CreateDefault().Deserialize<T>(reader);
        }

        /// <summary>
        /// Materialize as a JToken DOM for flexible member access.
        /// </summary>
        public JToken AsJToken() {
            if (data == null)
                return JValue.CreateNull();
            using var reader = CreateReader();
            return JToken.ReadFrom(reader);
        }

        /// <summary>
        /// Retrieve the raw JSON data as a span.
        /// </summary>
        public ReadOnlySpan<byte> AsSpan() => data;

        /// <summary>
        /// Decode the raw JSON data to a string.
        /// </summary>
        public override string ToString() => data != null ?
            Encoding.UTF8.GetString(data) :
            string.Empty; 
        #endregion


        #region --Operations--
        private readonly byte[]? data;

        private JsonTextReader CreateReader() {
            var stream = new MemoryStream(data, writable: false);
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            return new JsonTextReader(streamReader);
        }
        #endregion
    }
}