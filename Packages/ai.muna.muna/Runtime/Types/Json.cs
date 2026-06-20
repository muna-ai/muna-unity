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
            this.text = null; // decode lazily
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
        public bool IsObject => FirstNonWhitespaceByte() == (byte)'{';

        /// <summary>
        /// Whether the JSON is an array.
        /// </summary>
        public bool IsArray => FirstNonWhitespaceByte() == (byte)'[';

        /// <summary>
        /// Retrieve the raw JSON data as a span.
        /// </summary>
        public ReadOnlySpan<byte> AsSpan() => data;

        /// <summary>
        /// Create a JSON text reader over the JSON data.
        /// </summary>
        /// <returns></returns>
        public JsonTextReader CreateReader() {
            var stream = new MemoryStream(data, writable: false);
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            return new JsonTextReader(streamReader);
        }

        /// <summary>
        /// Deserialize the JSON object.
        /// </summary>
        public T? ToObject<T>() => ToObject<T>(Serializer);

        /// <summary>
        /// Deserialize the JSON object using a provided serializer.
        /// </summary>
        public T? ToObject<T>(JsonSerializer serializer) {
            if (data == null)
                return default;
            using var reader = CreateReader();
            return serializer.Deserialize<T>(reader);
        }

        /// <summary>
        /// Decode the raw JSON data to a string.
        /// </summary>
        public override string ToString() => 
            text ??
            (data != null ? Encoding.UTF8.GetString(data) : "undefined");

        /// <summary>
        /// Create a JSON object from a given object.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="value">Input value.</param>
        /// <returns>Serialized JSON object.</returns>
        public static Json From<T>(T value) => From<T>(value, Serializer);

        /// <summary>
        /// Create a JSON object from a given object.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="value">Input value.</param>
        /// <param name="serializer">Custom JSON serializer.</param>
        /// <returns>Serialized JSON object.</returns>
        public static Json From<T>(T value, JsonSerializer serializer) {
            using var stream = new MemoryStream();        // or pooled, if hot
            using (var streamWriter = new StreamWriter(stream, NoBomUtf8, 1024, leaveOpen: true))
            using (var jsonWriter = new JsonTextWriter(streamWriter)) {
                serializer.Serialize(jsonWriter, value);
            }
            return new Json(stream.ToArray());
        }
        #endregion


        #region --Operations--
        private readonly byte[]? data;
        private readonly string? text;
        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();
        private static readonly UTF8Encoding NoBomUtf8 = new(false);

        private byte FirstNonWhitespaceByte() {
            if (data == null)
                return 0;
            foreach (var b in data) {
                if (b != (byte)' ' && b != (byte)'\t' && b != (byte)'\n' && b != (byte)'\r')
                    return b;
            }
            return 0;
        }
        #endregion
    }
}