/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyCompany(@"NatML Inc")]
[assembly: AssemblyTitle(@"Muna.Unity")]
[assembly: AssemblyVersion(Muna.Muna.Version)]
[assembly: AssemblyCopyright(@"Copyright © 2025 NatML Inc. All Rights Reserved.")]
[assembly: InternalsVisibleTo(@"Muna.Editor")]
[assembly: InternalsVisibleTo(@"Muna.Tests.Editor")]
[assembly: InternalsVisibleTo(@"Muna.Tests.Runtime")]

namespace Muna {

    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using Unity.Collections.LowLevel.Unsafe;
    using API;
    using Beta.OpenAI;
    using Internal;

    /// <summary>
    /// Utilities for working with Unity.
    /// </summary>
    public static class MunaUnity {

        #region --Client API--
        /// <summary>
        /// Create a Muna client for Unity.
        /// </summary>
        /// <param name="accessKey">Muna access key. This defaults to your access key in Project Settings.</param>
        /// <param name="url">Muna API URL.</param>
        /// <returns>Muna client.</returns>
        public static Muna Create(
            string? accessKey = null,
            string? url = null
        ) {
            var settings = MunaSettings.Instance!;
            var client = new PredictionCacheClient(
                url ?? Muna.URL,
                accessKey: accessKey ?? settings?.accessKey
            );
            return new Muna(client);
        }

        /// <summary>
        /// Convert a texture to an image.
        /// NOTE: The texture format must be `R8`, `Alpha8`, `RGB24`, or `RGBA32`.
        /// </summary>
        /// <param name="texture">Input texture.</param>
        /// <param name="pixelBuffer">Pixel buffer to store image data. Use this to prevent allocations.</param>
        /// <returns>Image.</returns>
        public static unsafe Image ToImage(
            this Texture2D texture,
            byte[]? pixelBuffer = null
        ) {
            // Check texture
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            // Check that texture is readable
            if (!texture.isReadable)
                throw new InvalidOperationException(@"Texture cannot be converted to a Muna image because it is not readable");
            // Allocate buffer
            var channels = TextureFormatToImageChannels.GetValueOrDefault(texture.format, 4);
            var rowStride = texture.width * channels;
            var bufferSize = rowStride * texture.height;
            pixelBuffer ??= new byte[bufferSize];
            if (pixelBuffer.Length < bufferSize)
                throw new InvalidOperationException($"Texture cannot be converted to a Muna image because pixel buffer length was expected to be greater than or equal to {bufferSize} but got {pixelBuffer.Length}");
            // Copy
            var colorData = !TextureFormatToImageChannels.ContainsKey(texture.format) ? texture.GetPixels32() : null;
            fixed (void* dst = pixelBuffer, colors = colorData) {
                var src = colors == null ? texture.GetRawTextureData<byte>().GetUnsafePtr() : colors;
                UnsafeUtility.MemCpyStride(
                    dst,
                    rowStride,
                    (byte*)src + (rowStride * (texture.height - 1)),
                    -rowStride,
                    rowStride,
                    texture.height
                );
            }
            // Return
            return new Image(pixelBuffer, texture.width, texture.height, channels);
        }

        /// <summary>
        /// Convert an image to a texture.
        /// </summary>
        /// <param name="value">Image.</param>
        /// <param name="texture">Optional destination texture.</param>
        /// <returns>Texture.</returns>
        public static unsafe Texture2D ToTexture(
            this Image image,
            Texture2D? texture = null
        ) {
            if (!ImageChannelsToTextureFormat.TryGetValue(image.channels, out var format))
                throw new InvalidOperationException($"Image cannot be converted to a Texture2D because it has unsupported channel count: {image.channels}");
            texture = texture != null ? texture : new Texture2D(image.width, image.height, format, false);
            if (texture.width != image.width || texture.height != image.height || texture.format != format)
                texture.Reinitialize(image.width, image.height, format, false);
            var rowStride = image.width * image.channels;
            fixed (byte* srcData = image)
                UnsafeUtility.MemCpyStride(
                    texture.GetRawTextureData<byte>().GetUnsafePtr(),
                    rowStride,
                    srcData + (rowStride * (image.height - 1)),
                    -rowStride,
                    rowStride,
                    image.height
                );
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Convert a `BinaryData` containing linear PCM audio into an audio clip.
        /// </summary>
        /// <param name="data">Binary data containing linear PCM audio.</param>
        /// <returns>Audio clip.</returns>
        public static unsafe AudioClip ToAudioClip(this BinaryData data) {
            // Check that this contains LPCM data
            if (string.IsNullOrEmpty(data.MediaType) || !data.MediaType.StartsWith(@"audio/pcm"))
                throw new ArgumentException($"Failed to create audio clip from binary data because media type was expected to be 'audio/pcm' but got: '{data.MediaType}'");
            // Match sample rate and channel count
            var rateMatch = Regex.Match(data.MediaType, @"rate=(\d+)");
            var channelsMatch = Regex.Match(data.MediaType, @"channels=(\d+)");
            if (!rateMatch.Success || !channelsMatch.Success)
                throw new ArgumentException($"Failed to create audio clip from binary data because media type is invalid: '{data.MediaType}'");
            // Parse
            if (!int.TryParse(rateMatch.Groups[1].Value, out var sampleRate))
                throw new ArgumentException($"Failed to create audio clip from binary data because sample rate is invalid: '{rateMatch.Value}'");
            if (!int.TryParse(channelsMatch.Groups[1].Value, out var channelCount))
                throw new ArgumentException($"Failed to create audio clip from binary data because channel count is invalid: '{channelsMatch.Value}'");
            // Create clip
            var sampleCount = data.Length / sizeof(float);
            var frameCount = sampleCount / channelCount;
            var clip = AudioClip.Create(
                "audio",
                lengthSamples: frameCount,
                channels: channelCount,
                frequency: sampleRate,
                stream: false
            );
            // Copy data
            var samples = new float[sampleCount];
            Buffer.BlockCopy(data.ToArray(), 0, samples, 0, data.Length);
            clip.SetData(samples, 0);
            // Return
            return clip;
        }
        #endregion


        #region --Operations--
        private static readonly Dictionary<TextureFormat, int> TextureFormatToImageChannels = new() {
            [TextureFormat.R8] = 1,
            [TextureFormat.Alpha8] = 1,
            [TextureFormat.RGB24] = 3,
            [TextureFormat.RGBA32] = 4,
        };
        private static readonly Dictionary<int, TextureFormat> ImageChannelsToTextureFormat = new() {
            [1] = TextureFormat.Alpha8,
            [3] = TextureFormat.RGB24,
            [4] = TextureFormat.RGBA32,
        };
        #endregion
    }
}