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
    /// Convert an array field to a `Vector3`.
    /// The array MUST contain exactly three numbers.
    /// </summary>
    public sealed class ArrayToVector3Converter : JsonConverter<Vector3> {

        public override void WriteJson(
            JsonWriter writer,
            Vector3 value,
            JsonSerializer serializer
        ) {
            var obj = new JArray { value.x, value.y, value.z };
            obj.WriteTo(writer);
        }

        public override Vector3 ReadJson(
            JsonReader reader,
            Type type,
            Vector3 existing,
            bool hasExisting,
            JsonSerializer s
        ) {
            var arr = JArray.Load(reader);
            return new Vector3(
                x: (float)arr[0],
                y: (float)arr[1], 
                z: (float)arr[2]
            );
        }
    }
}