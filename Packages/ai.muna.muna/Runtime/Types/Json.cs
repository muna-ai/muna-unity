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
        public Json(byte[] data) {
            this.data = data;
            this.text = Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Create the JSON object from a string.
        /// </summary>
        public Json(string text) {
            this.data = Encoding.UTF8.GetBytes(text);
            this.text = text;
        }

        /// <summary>
        /// Whether the JSON is an object.
        /// </summary>
        public bool IsObject => text.AsSpan().TrimStart().StartsWith("{");

        /// <summary>
        /// Whether the JSON is an array.
        /// </summary>
        public bool IsArray => text.AsSpan().TrimStart().StartsWith("[");

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
        public override string ToString() => text ?? "undefined"; 
        #endregion


        #region --Operations--
        private readonly byte[]? data;
        private readonly string? text;

        private JsonTextReader CreateReader() {
            var stream = new MemoryStream(data, writable: false);
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            return new JsonTextReader(streamReader);
        }
        #endregion
    }
}