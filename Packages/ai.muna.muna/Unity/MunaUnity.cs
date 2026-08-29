/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyCompany(@"NatML Inc")]
[assembly: AssemblyTitle(@"Muna.Unity")]
[assembly: AssemblyVersion(Muna.Muna.Version)]
[assembly: AssemblyCopyright(@"Copyright © 2026 NatML Inc. All Rights Reserved.")]
[assembly: InternalsVisibleTo(@"Muna.Editor")]
[assembly: InternalsVisibleTo(@"Muna.Tests.Editor")]
[assembly: InternalsVisibleTo(@"Muna.Tests.Runtime")]

namespace Muna {

    using System;
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
            var client = new UnityClient(
                url ?? Muna.URL,
                accessKey: accessKey ?? settings?.accessKey
            );
            var muna = new Muna(client);
            foreach (var prediction in settings?.cache ?? new())
                muna.Predictions.cache.Pin(
                    prediction.tag,
                    prediction.target,
                    prediction.id
                );
            return muna;
        }

        /// <summary>
        /// Copy pixel data from a texture into an image.
        /// NOTE: The texture format must be `R8`, `Alpha8`, `RGB24`, or `RGBA32`.
        /// </summary>
        /// <param name="texture">Texture to copy pixel data from.</param>
        /// <param name="image">Image to copy pixel data to.</param>
        public static unsafe void CopyTo(
            this Texture2D texture,
            Image image
        ) {
            // Check texture
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            // Check that texture is readable
            if (!texture.isReadable)
                throw new ArgumentException(@"Cannot copy pixel data from texture because it is not readable");
            // Check dims
            if (image.width != texture.width || image.height != texture.height)
                throw new ArgumentException($"Cannot copy {texture.width}x{texture.height} texture into {image.width}x{image.height} image");
            // Check channels
            var channels = ChannelsForFormat(texture.format);
            if (channels == 0)
                throw new ArgumentException($"Cannot convert texture to image because texture format is {texture.format} but expected [R8, Alpha8, RGB24, RGBA32]");
            if (image.channels != channels)
                throw new ArgumentException($"Cannot copy {texture.format} texture into {image.channels} channel image");
            // Copy
            var rowStride = texture.width * channels;
            fixed (void* dst = image)
                UnsafeUtility.MemCpyStride(
                    (byte*)dst + (rowStride * (image.height - 1)),
                    -rowStride,
                    texture.GetRawTextureData<byte>().GetUnsafePtr(),
                    rowStride,
                    rowStride,
                    image.height
                );
        }
        
        /// <summary>
        /// Copy pixel data from an image into a texture.
        /// NOTE: The texture format must be `R8`, `Alpha8`, `RGB24`, or `RGBA32`.
        /// </summary>
        /// <param name="image">Image to copy pixel data from.</param>
        /// <param name="texture">Texture to copy pixel data to.</param>
        public static unsafe void CopyTo(
            this Image image,
            Texture2D texture
        ) {
            // Check texture
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            // Check that texture is readable
            if (!texture.isReadable)
                throw new ArgumentException(@"Cannot copy pixel data to texture because it is not readable");
            // Check dims
            if (image.width != texture.width || image.height != texture.height)
                throw new ArgumentException($"Cannot copy {image.width}x{image.height} image into {texture.width}x{texture.height} texture");
            // Check channels
            if (image.channels != ChannelsForFormat(texture.format))
                throw new ArgumentException($"Cannot copy {image.channels} channel image into {texture.format} texture");
            // Copy
            var rowStride = image.width * image.channels;
            fixed (byte* src = image)
                UnsafeUtility.MemCpyStride(
                    texture.GetRawTextureData<byte>().GetUnsafePtr(),
                    rowStride,
                    src + (rowStride * (image.height - 1)),
                    -rowStride,
                    rowStride,
                    image.height
                );
        }

        /// <summary>
        /// Convert a texture to an image.
        /// NOTE: The texture format must be `R8`, `Alpha8`, `RGB24`, or `RGBA32`.
        /// </summary>
        /// <param name="texture">Input texture.</param>
        /// <returns>Image.</returns>
        public static Image ToImage(this Texture2D texture) {
            // Check channels
            var channels = ChannelsForFormat(texture.format);
            if (channels == 0)
                throw new ArgumentException($"Cannot convert texture to image because texture format is {texture.format} but expected [R8, Alpha8, RGB24, RGBA32]");
            // Copy
            var image = new Image(
                data: new byte[texture.width * texture.height * channels],
                width: texture.width,
                height: texture.height,
                channels: channels
            );
            CopyTo(texture, image);
            // Return
            return image;
        }

        /// <summary>
        /// Convert an image to a texture.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>Texture.</returns>
        public static Texture2D ToTexture(this Image image) {
            var texture = new Texture2D(
                image.width,
                image.height,
                FormatForChannels(image.channels),
                false
            );
            CopyTo(image, texture);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Convert a `BinaryData` containing linear PCM audio into an audio clip.
        /// </summary>
        /// <param name="data">Binary data containing linear PCM audio.</param>
        /// <returns>Audio clip.</returns>
        public static AudioClip ToAudioClip(this BinaryData data) {
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

        private static int ChannelsForFormat(TextureFormat format) => format switch {
            TextureFormat.R8        => 1,
            TextureFormat.Alpha8    => 1,
            TextureFormat.RGB24     => 3,
            TextureFormat.RGBA32    => 4,
            _                       => 0,  
        };

        private static TextureFormat FormatForChannels(int channels) => channels switch {
            1 => TextureFormat.Alpha8,
            3 => TextureFormat.RGB24,
            4 => TextureFormat.RGBA32,
            _ => 0
        };
        #endregion
    }
}