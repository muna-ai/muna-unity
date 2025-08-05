/*
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Types {

    using Newtonsoft.Json;

    /// <summary>
    /// Image.
    /// </summary>
    [Preserve]
    public unsafe readonly struct Image {

        #region --Client API--
        /// <summary>
        /// Image pixel buffer.
        /// This is always 8bpp interleaved by channel.
        /// </summary>
        [JsonIgnore]
        public readonly byte[] data;

        /// <summary>
        /// Image width.
        /// </summary>
        public readonly int width;

        /// <summary>
        /// Image height.
        /// </summary>
        public readonly int height;

        /// <summary>
        /// Image channels.
        /// </summary>
        public readonly int channels;
        
        /// <summary>
        /// Create an image.
        /// </summary>
        /// <param name="data">Pixel buffer. The pixel buffer format MUST be `R8`, `RGB888`, or `RGBA8888`.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="channels">Image channels.</param>
        public Image (byte[] data, int width, int height, int channels) {
            this.data = data;
            this.nativeData = null;
            this.width = width;
            this.height = height;
            this.channels = channels;
        }

        /// <summary>
        /// Create an image from a pixel buffer.
        /// NOTE: DO NOT use this overload unless you absolutely know what you are doing.
        /// </summary>
        /// <param name="data">Pixel buffer. The pixel buffer format MUST be `R8`, `RGB888`, or `RGBA8888`.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height</param>
        /// <param name="channels">Image channels.</param>
        public unsafe Image (byte* data, int width, int height, int channels) { // Enables zero copy into `FXNValue`
            this.data = null!;
            this.nativeData = data;
            this.width = width;
            this.height = height;
            this.channels = channels;
        }
        #endregion


        #region --Operations--
        private readonly byte* nativeData;

        [JsonProperty(@"data"), Preserve]
        private readonly string dataPlaceholder {
            get {
                var channelStr = "?";
                switch (channels) {
                    case 1: channelStr = "R8"; break;
                    case 3: channelStr = "RGB888"; break;
                    case 4: channelStr = "RGBA8888"; break;
                }
                return $"<{width}x{height},{channelStr},{data.Length} bytes>";
            }
        }
        
        public ref byte GetPinnableReference () => ref (nativeData == null ? ref data[0] : ref *nativeData);
        #endregion
    }
}