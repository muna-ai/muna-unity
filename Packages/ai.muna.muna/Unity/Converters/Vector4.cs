/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Converters {

    using System;
    using UnityEngine;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Convert an array field to a `Vector4`.
    /// The array MUST contain exactly four numbers.
    /// </summary>
    public sealed class ArrayToVector4Converter : JsonConverter<Vector4> {

        public override void WriteJson(
            JsonWriter writer,
            Vector4 value,
            JsonSerializer serializer
        ) {
            var obj = new JArray { value.x, value.y, value.z, value.w };
            obj.WriteTo(writer);
        }

        public override Vector4 ReadJson(
            JsonReader reader,
            Type type,
            Vector4 existing,
            bool hasExisting,
            JsonSerializer s
        ) {
            var arr = JArray.Load(reader);
            return new Vector4(
                x: (float)arr[0],
                y: (float)arr[1],
                z: (float)arr[2],
                w: (float)arr[3]
            );
        }
    }
}