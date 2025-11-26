/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Converters {

    using System;
    using UnityEngine;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Convert an array field to a `Color`.
    /// The array MUST contain three or four numbers.
    /// </summary>
    public sealed class ArrayToColorConverter : JsonConverter<Color> {

        public override void WriteJson(
            JsonWriter writer,
            Color value,
            JsonSerializer serializer
        ) {
            var obj = new JArray { value.r, value.g, value.b, value.a };
            obj.WriteTo(writer);
        }

        public override Color ReadJson(
            JsonReader reader,
            Type type,
            Color existing,
            bool hasExisting,
            JsonSerializer s
        ) {
            var arr = JArray.Load(reader);
            return new Color(
                r: (float)arr[0],
                g: (float)arr[1],
                b: (float)arr[2],
                a: arr.Count > 3 ? (float)arr[3] : 1f
            );
        }
    }
}