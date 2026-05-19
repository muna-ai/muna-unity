/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.C {

    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
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

        public Dtype dtype {
            get {
                value.GetValueType(out var dtype).Throw();
                return dtype;
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

        public object? ToObject() => dtype switch {
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
            Dtype.List      => new Json(ToArray((byte*)data, GetUtf8Length(data))),
            Dtype.Dict      => new Json(ToArray((byte*)data, GetUtf8Length(data))),
            Dtype.Image     => new Image(ToArray((byte*)data, shape), shape[1], shape[0], shape[2]),
            Dtype.Binary    => new MemoryStream(ToArray((byte*)data, shape)),
            _               => throw new InvalidOperationException($"Cannot convert Muna value to object because value type is unsupported: {dtype}"),
        };

        public byte[] Serialize(string contentType) {
            CreateSerializedValue(value, contentType, out var result).Throw();
            using var serialized = new Value(result);
            return ToArray((byte*)serialized.data, serialized.shape);
        }

        public void Dispose() => value.ReleaseValue();

        public static Value CreateArray<T>(T scalar) where T : unmanaged => CreateArray(
            new Tensor<T>(new [] { scalar }, new int[0]),
            Flags.CopyData
        );

        public static Value CreateArray<T>(T[] vector) where T : unmanaged => CreateArray(
            new Tensor<T>(vector, new [] { vector.Length }),
            Flags.CopyData
        );

        public static Value CreateArray<T>(
            in Tensor<T> tensor,
            Flags flags = Flags.None
        ) where T : unmanaged {
            IntPtr value = default;
            flags |= tensor.data != null ? Flags.CopyData : 0; // GC can move managed memory
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

        public static Value CreateString(string input) {
            CreateStringValue(input, out var value).Throw();
            return new Value(value);
        }

        public static Value CreateList(IList list) {
            var json = JsonConvert.SerializeObject(list);
            CreateListValue(json, out var value).Throw();
            return new Value(value);
        }

        public static Value CreateDict(IDictionary dict) {
            var json = JsonConvert.SerializeObject(dict);
            CreateDictValue(json, out var value).Throw();
            return new Value(value);
        }

        public static Value CreateImage(
            in Image image,
            Flags flags = Flags.None
        ) {
            IntPtr value = default;
            flags |= image.data != null ? Flags.CopyData : 0; // GC can move managed memory
            fixed (byte* data = image)
                CreateImageValue(
                    data,
                    image.width,
                    image.height,
                    image.channels,
                    flags,
                    out value
                ).Throw();
            return new Value(value);
        }

        public static Value CreateBinary(
            Stream stream,
            Flags flags = Flags.None
        ) {
            byte[] data;
            if (stream is MemoryStream memoryStream)
                data = memoryStream.ToArray();
            else {
                using var dstStream = new MemoryStream();
                stream.CopyTo(dstStream);
                data = dstStream.ToArray();
            }
            flags |= Flags.CopyData;
            CreateBinaryValue(
                data,
                data.Length,
                flags,
                out var value
            ).Throw();
            return new Value(value);
        }

        public static Value CreateNull() {
            CreateNullValue(out var value).Throw();
            return new Value(value);
        }

        public static Value CreateFromBinary(
            Stream stream,
            string contentType
        ) {
            using var binaryValue = CreateBinary(stream);
            CreateValueFromSerializedValue(
                binaryValue,
                contentType,
                out var result
            ).Throw();
            return new Value(result);
        }
        #endregion


        #region --Operations--
        private readonly IntPtr value;

        internal Value(IntPtr value) => this.value = value;

        public static implicit operator IntPtr(Value value) => value.value;

        private static unsafe object ToObject<T>(T* data, int[] shape) where T : unmanaged {
            if (shape.Length == 0)
                return *(T*)data;
            var array = ToArray(data, shape);
            return new Tensor<T>(array, shape);
        }

        private static unsafe T[] ToArray<T>(T* data, int[] shape) where T : unmanaged {
            var length = shape.Aggregate(1, (a, b) => a * b);
            return ToArray(data, length);
        }

        private static unsafe T[] ToArray<T>(T* data, int length) where T : unmanaged {
            var result = new T[length];
            var size = length * sizeof(T);
            fixed (void* dst = result)
                Buffer.MemoryCopy(data, dst, size, size);
            return result;
        }

        private static Dtype ToDtype<T>() where T : unmanaged => default(T) switch { // don't use this for reference types
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

        private static int GetUtf8Length(void* ptr) {
            if (ptr == null)
                return 0;
            var len = 0;
            var data = (byte*)ptr;
            while (data[len] != 0)
                len++;
            return len;
        }
        #endregion
    }
}