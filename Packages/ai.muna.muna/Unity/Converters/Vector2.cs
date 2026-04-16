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
    /// Convert an array field to a `Vector2`.
    /// The array MUST contain exactly two numbers.
    /// </summary>
    public sealed class ArrayToVector2Converter : JsonConverter<Vector2> {

        public override void WriteJson(
            JsonWriter writer,
            Vector2 value,
            JsonSerializer serializer
        ) {
            var obj = new JArray { value.x, value.y };
            obj.WriteTo(writer);
        }

        public override Vector2 ReadJson(
            JsonReader reader,
            Type type,
            Vector2 existing,
            bool hasExisting,
            JsonSerializer s
        ) {
            var arr = JArray.Load(reader);
            return new Vector2(
                x: (float)arr[0],
                y: (float)arr[1]
            );
        }
    }
}