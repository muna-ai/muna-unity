/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta {

    using Newtonsoft.Json;

    /// <summary>
    /// Audio buffer.
    /// </summary>
    [Preserve]
    public unsafe readonly struct Audio {

        #region --Client API--
        /// <summary>
        /// Linear PCM audio samples with shape `(F,C)`.
        /// </summary>
        [JsonIgnore]
        public readonly float[] samples;

        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public readonly int sampleRate;

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public readonly int channelCount;

        /// <summary>
        /// Audio sample count.
        /// </summary>
        public readonly int sampleCount;

        /// <summary>
        /// Create an audio buffer.
        /// </summary>
        /// <param name="samples">Audio samples. MUST be linear PCM interleaved by channel.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        /// <param name="channelCount">Audio channel count.</param>
        public Audio(float[] samples, int sampleRate, int channelCount) {
            this.samples = samples;
            this.nativeSamples = null;
            this.sampleRate = sampleRate;
            this.channelCount = channelCount;
            this.sampleCount = samples.Length;
        }

        /// <summary>
        /// Create an audio buffer.
        /// </summary>
        /// <param name="samples">Audio samples. MUST be linear PCM interleaved by channel.</param>
        /// <param name="sampleCount">Audio sample count.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        /// <param name="channelCount">Audio channel count.</param>
        public Audio( // Enables zero copy into `FXNValue`
            float* samples,
            int sampleCount,
            int sampleRate,
            int channelCount
        ) {
            this.samples = null!;
            this.nativeSamples = samples;
            this.sampleRate = sampleRate;
            this.channelCount = channelCount;
            this.sampleCount = sampleCount;
        }
        #endregion


        #region --Operations--
        private readonly float* nativeSamples;

        public ref float GetPinnableReference() => ref (nativeSamples == null ? ref samples[0] : ref *nativeSamples);

        internal Tensor<float> AsTensor() {
            var frameCount = sampleCount / channelCount;
            var shape = new[] { frameCount, channelCount };
            return nativeSamples != null ?
                new Tensor<float>(nativeSamples, shape) :
                new Tensor<float>(samples, shape);
        }
        #endregion
    }
}