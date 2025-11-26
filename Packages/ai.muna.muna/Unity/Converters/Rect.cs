/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Converters {

    using System;
    using System.Runtime.Serialization;
    using UnityEngine;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Box format.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BoxFormat : int {
        /// <summary>
        /// Boxes are represented via corners: x1, y1 being top left and x2, y2 being bottom right.
        /// </summary>
        [EnumMember(Value = @"xyxy")]
        XYXY = 1,
        /// <summary>
        /// Boxes are represented via corner, width and height: x1, y2 being top left.
        /// </summary>
        [EnumMember(Value = @"xywh")]
        XYWH = 2,
        /// <summary>
        /// Boxes are represented via center, width, and height.
        /// </summary>
        [EnumMember(Value = @"cxcywh")]
        CxCyWH = 3,
        /// <summary>
        /// Boxes are represented via corners:
        /// - x1, y1 being top left
        /// - x2, y2 top right
        /// - x3, y3 bottom right
        /// - x4, y4 bottom left
        /// </summary>
        [EnumMember(Value = @"xyxyxyxy")]
        XYXYXYXY = 4,
    }

    /// <summary>
    /// Convert an object field to a `Rect`.
    /// </summary>
    public sealed class ObjectToRectConverter : JsonConverter<Rect> {

        private readonly BoxFormat format;
        private readonly string[] fieldNames;

        /// <summary>
        /// Create a converter with the provided box format.
        /// </summary>
        /// <param name="format">Box format to parse.</param>
        public ObjectToRectConverter(BoxFormat format): this(
            format,
            GetReferenceFieldNames(format)
        ) { }

        /// <summary>
        /// Create a converter with the provided box format and corresponding field names.
        /// </summary>
        /// <param name="format">Box format to parse.</param>
        /// <param name="fieldNames">Field names that correspond to the chosen format.</param>
        public ObjectToRectConverter(BoxFormat format, string[] fieldNames) {
            this.format = format;
            this.fieldNames = fieldNames;
        }

        public override void WriteJson(
            JsonWriter writer,
            Rect value,
            JsonSerializer serializer
        ) {
            var values = GetRectValues(value, format);
            var obj = new JObject();
            for (var i = 0; i < fieldNames.Length; ++i)
                obj[fieldNames[i]] = values[i];
            obj.WriteTo(writer);
        }

        public override Rect ReadJson(
            JsonReader reader,
            Type type,
            Rect existing,
            bool hasExisting,
            JsonSerializer s
        ) {
            var obj = JObject.Load(reader);
            return format switch {
                BoxFormat.XYXY => Rect.MinMaxRect(
                    xmin: GetFieldValue(obj, fieldNames[0]),
                    ymin: GetFieldValue(obj, fieldNames[1]),
                    xmax: GetFieldValue(obj, fieldNames[2]),
                    ymax: GetFieldValue(obj, fieldNames[3])
                ),
                BoxFormat.XYWH => new Rect(
                    x: GetFieldValue(obj, fieldNames[0]),
                    y: GetFieldValue(obj, fieldNames[1]),
                    width: GetFieldValue(obj, fieldNames[2]),
                    height: GetFieldValue(obj, fieldNames[3])
                ),
                BoxFormat.CxCyWH => GetCenterRect(obj),
                BoxFormat.XYXYXYXY => Rect.MinMaxRect(
                    xmin: GetFieldValue(obj, fieldNames[0]),  // left
                    ymin: GetFieldValue(obj, fieldNames[1]),  // top
                    xmax: GetFieldValue(obj, fieldNames[4]),  // right
                    ymax: GetFieldValue(obj, fieldNames[5])   // bottom
                ),
                _ => throw new JsonSerializationException($"Failed to read `Rect` from JSON object because of unsupported format: {format}")
            };
        }

        private Rect GetCenterRect(JObject obj) {
            var cx = GetFieldValue(obj, fieldNames[0]);
            var cy = GetFieldValue(obj, fieldNames[1]);
            var w = GetFieldValue(obj, fieldNames[2]);
            var h = GetFieldValue(obj, fieldNames[3]);
            var center = new Vector2(cx, cy);
            var size = new Vector2(w, h);
            return new Rect(center - 0.5f * size, size);
        }

        private float GetFieldValue(
            JObject obj,
            string name
        ) {
            if (!obj.TryGetValue(name, StringComparison.InvariantCulture, out var value))
                throw new JsonSerializationException($"Missing '{name}' field for {format} box.");
            return (float)value;
        }

        internal static float[] GetRectValues(
            in Rect rect,
            BoxFormat format
        ) => format switch {
            BoxFormat.XYXY      => new[] { rect.xMin, rect.yMin, rect.xMax, rect.yMax },
            BoxFormat.XYWH      => new[] { rect.xMin, rect.yMin, rect.width, rect.height },
            BoxFormat.CxCyWH    => new[] { rect.center.x, rect.center.y, rect.width, rect.height },
            BoxFormat.XYXYXYXY  => new[] {
                rect.xMin, rect.yMin, // top-left
                rect.xMax, rect.yMin, // top-right
                rect.xMax, rect.yMax, // bottom-right
                rect.xMin, rect.yMax  // bottom-left
            },
            _                   => throw new ArgumentOutOfRangeException(nameof(format))
        };

        private static string[] GetReferenceFieldNames(BoxFormat format) => format switch {
            BoxFormat.XYXY      => new[] { @"x_min", @"y_min", @"x_max", @"y_max" },
            BoxFormat.XYWH      => new[] { @"x", @"y", @"width", @"height" },
            BoxFormat.CxCyWH    => new[] { @"x_center", @"y_center", @"width", @"height" },
            BoxFormat.XYXYXYXY  => new[] { @"x1", @"y1", @"x2", @"y2", @"x3", @"y3", @"x4", @"y4" },
            _                   => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    /// <summary>
    /// Convert an array field to a `Rect`.
    /// The array MUST contain numbers, and have the required count depending on the box format.
    /// </summary>
    public sealed class ArrayToRectConverter : JsonConverter<Rect> {

        private readonly BoxFormat format;

        /// <summary>
        /// Create a converter for the provided box format.
        /// </summary>
        /// <param name="format">Box format to parse.</param>
        public ArrayToRectConverter(BoxFormat format) => this.format = format;

        public override void WriteJson(
            JsonWriter writer,
            Rect value,
            JsonSerializer serializer
        ) {
            var values = ObjectToRectConverter.GetRectValues(value, format);
            var arr = new JArray(values);
            arr.WriteTo(writer);
        }

        public override Rect ReadJson(
            JsonReader reader,
            Type type,
            Rect existing,
            bool hasExisting,
            JsonSerializer s
        ) {
            var arr = JArray.Load(reader);
            var expected = ExpectedCount(format);
            if (arr.Count != expected)
                throw new JsonSerializationException($"Expected {expected} numbers for {format} box but got {arr.Count}.");
            var data = arr.ToObject<float[]>()!;
            return format switch {
                BoxFormat.XYXY => Rect.MinMaxRect(
                    xmin: data[0],
                    ymin: data[1],
                    xmax: data[2],
                    ymax: data[3]
                ),
                BoxFormat.XYWH => new Rect(
                    x: data[0],
                    y: data[1],
                    width: data[2],
                    height: data[3]
                ),
                BoxFormat.CxCyWH => new Rect(
                    x: data[0] - 0.5f * data[2],
                    y: data[1] * 0.5f - data[3],
                    width: data[2],
                    height: data[3]
                ),
                _ => throw new JsonSerializationException($"Failed to read `Rect` from JSON array because of unsupported format: {format}")
            };
        }

        private static int ExpectedCount(BoxFormat format) => format switch {
            BoxFormat.XYXY      => 4,
            BoxFormat.XYWH      => 4,
            BoxFormat.CxCyWH    => 4,
            BoxFormat.XYXYXYXY  => 8,
            _                   => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }
}
