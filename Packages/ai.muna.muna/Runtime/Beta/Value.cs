/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Beta {

    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Text;
    using Types;

    /// <summary>
    /// Remote prediction value.
    /// </summary>
    [Preserve, Serializable]
    internal class Value {

        /// <summary>
        /// Value URL.
        /// </summary>
        public string? data;

        /// <summary>
        /// Value type.
        /// </summary>
        public Dtype type;

        /// <summary>
        /// Value shape.
        /// This is `null` if shape information is not available or applicable.
        /// </summary>
        public int[]? shape;
    }

    internal static class ValueUtils {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ToObject<T> (this Stream stream, int[] shape) where T : unmanaged {
            var data = stream.ToArray<T>();
            return shape.Length > 0 ? new Tensor<T>(data, shape) : data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ToObject (this Enum value) {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attribute = fieldInfo?
                .GetCustomAttributes(typeof(EnumMemberAttribute), false)?
                .FirstOrDefault() as EnumMemberAttribute;
            return (attribute?.IsValueSetExplicitly ?? false) ? attribute.Value : Convert.ToInt32(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Stream ToStream<T> (this T[] data) where T : unmanaged {
            if (data is byte[] raw)
                return new MemoryStream(raw);
            var size = data.Length * sizeof(T);
            var array = new byte[size];
            fixed (void* src = data, dst = array)
                Buffer.MemoryCopy(src, dst, size, size);
            return new MemoryStream(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream ToStream (this string data) => new MemoryStream(Encoding.UTF8.GetBytes(data));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T[] ToArray<T> (this Stream stream) where T : unmanaged {
            var result = new T[stream.Length / sizeof(T)];
            fixed (T* dst = result) 
                using (var dstStream = new UnmanagedMemoryStream(
                    (byte*)dst,
                    stream.Length,
                    stream.Length,
                    FileAccess.Write
                ))
                    stream.CopyTo(dstStream);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream Clone (this Stream stream) {
            var result = new MemoryStream();
            stream.CopyTo(result);
            return result;
        }
    }
}