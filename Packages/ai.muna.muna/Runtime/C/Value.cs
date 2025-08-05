/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.C {

    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Types;
    using static Function;

    public unsafe sealed class Value : IDisposable {

        #region --Enumerations--
        [Flags]
        public enum Flags : int {
            None = 0,
            CopyData = 1,
        }
        #endregion


        #region --Client API--

        public void* data {
            get {
                value.GetValueData(out var data).Throw();
                return (void*)data;
            }
        }

        public Dtype type {
            get {
                value.GetValueType(out var type).Throw();
                return type;
            }
        }

        public int[] shape {
            get {
                value.GetValueDimensions(out var dims).Throw();
                var shape = new int[dims];
                value.GetValueShape(shape, dims).Throw();
                return shape;
            }
        }

        public object? ToObject () => type switch {
            Dtype.Null      => null,
            Dtype.Float32   => ToObject((float*)data, shape),
            Dtype.Float64   => ToObject((double*)data, shape),
            Dtype.Int8      => ToObject((sbyte*)data, shape),
            Dtype.Int16     => ToObject((short*)data, shape),
            Dtype.Int32     => ToObject((int*)data, shape),
            Dtype.Int64     => ToObject((long*)data, shape),
            Dtype.Uint8     => ToObject((byte*)data, shape),
            Dtype.Uint16    => ToObject((ushort*)data, shape),
            Dtype.Uint32    => ToObject((uint*)data, shape),
            Dtype.Uint64    => ToObject((ulong*)data, shape),
            Dtype.Bool      => ToObject((bool*)data, shape),
            Dtype.String    => Marshal.PtrToStringUTF8((IntPtr)data),
            Dtype.List      => JsonConvert.DeserializeObject<JArray>(Marshal.PtrToStringUTF8((IntPtr)data)),
            Dtype.Dict      => JsonConvert.DeserializeObject<JObject>(Marshal.PtrToStringUTF8((IntPtr)data)),
            Dtype.Image     => new Image(ToArray((byte*)data, shape), shape[1], shape[0], shape[2]),
            Dtype.Binary    => new MemoryStream(ToArray((byte*)data, shape)),
            _               => throw new InvalidOperationException($"Cannot convert Function value to object because value type is unsupported: {type}"),
        };

        public void Dispose () => value.ReleaseValue();

        public static Value CreateArray<T> (T scalar) where T : unmanaged => CreateArray(
            new Tensor<T>(new [] { scalar }, new int[0]),
            Flags.CopyData
        );

        public static Value CreateArray<T> (T[] vector) where T : unmanaged => CreateArray(
            new Tensor<T>(vector, new [] { vector.Length }),
            Flags.CopyData
        );

        public static Value CreateArray<T> (
            in Tensor<T> tensor,
            Flags flags = Flags.None
        ) where T : unmanaged {
            IntPtr value = default;
            fixed (T* data = tensor)
                CreateArrayValue(
                    data,
                    tensor.shape,
                    tensor.shape.Length,
                    ToDtype<T>(),
                    flags,
                    out value
                ).Throw();
            return new Value(value);
        }

        public static Value CreateString (string input) {
            CreateStringValue(input, out var value).Throw();
            return new Value(value);
        }

        public static Value CreateList (IList list) {
            var json = JsonConvert.SerializeObject(list);
            CreateListValue(json, out var value).Throw();
            return new Value(value);
        }

        public static Value CreateDict (IDictionary dict) {
            var json = JsonConvert.SerializeObject(dict);
            CreateDictValue(json, out var value).Throw();
            return new Value(value);
        }

        public static Value CreateImage (in Image image) {
            IntPtr value = default;
            fixed (byte* data = image)
                CreateImageValue(
                    data,
                    image.width,
                    image.height,
                    image.channels,
                    image.data != null ? Flags.CopyData : Flags.None,
                    out value
                ).Throw();
            return new Value(value);
        }

        public static Value CreateBinary (Stream stream) {
            byte[] data;
            if (stream is MemoryStream memoryStream)
                data = memoryStream.ToArray();
            else {
                using var dstStream = new MemoryStream();
                stream.CopyTo(dstStream);
                data = dstStream.ToArray();
            }
            CreateBinaryValue(data, data.Length, Flags.CopyData, out var value).Throw();
            return new Value(value);
        }

        public static Value CreateNull () {
            CreateNullValue(out var value).Throw();
            return new Value(value);
        }
        #endregion


        #region --Operations--
        private readonly IntPtr value;

        internal Value (IntPtr value) => this.value = value;

        public static implicit operator IntPtr (Value value) => value.value;

        private static unsafe object ToObject<T> (T* data, int[] shape) where T : unmanaged {
            if (shape.Length == 0)
                return *(T*)data;
            var array = ToArray(data, shape);
            return new Tensor<T>(array, shape);
        }

        private static unsafe T[] ToArray<T> (T* data, int[] shape) where T : unmanaged {
            var count = shape.Aggregate(1, (a, b) => a * b);
            var result = new T[count];
            fixed (void* dst = result)
                Buffer.MemoryCopy(data, dst, count * sizeof(T), count * sizeof(T));
            return result;
        }

        private static Dtype ToDtype<T> () where T : unmanaged => default(T) switch { // don't use this for reference types
            float   _ => Dtype.Float32,
            double  _ => Dtype.Float64,
            sbyte   _ => Dtype.Int8,
            short   _ => Dtype.Int16,
            int     _ => Dtype.Int32,
            long    _ => Dtype.Int64,
            byte    _ => Dtype.Uint8,
            ushort  _ => Dtype.Uint16,
            uint    _ => Dtype.Uint32,
            ulong   _ => Dtype.Uint64,
            bool    _ => Dtype.Bool,
                    _ => Dtype.Null,
        };
        #endregion
    }
}