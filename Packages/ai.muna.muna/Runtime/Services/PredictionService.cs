/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Services {

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using API;

    /// <summary>
    /// Make predictions.
    /// </summary>
    public sealed class PredictionService {

        #region --Client API--
        /// <summary>
        /// Create a prediction.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <param name="device">Prediction device. Do not set this unless you know what you are doing.</param>
        /// <param name="clientId">Muna client identifier. Specify this to override the current client identifier.</param>
        /// <param name="configurationId">Configuration identifier. Specify this to override the current client configuration token.</param>
        public Task<Prediction> Create(
            string tag,
            Dictionary<string, object?>? inputs = default,
            string? acceleration = default,
            IntPtr device = default,
            string? clientId = default,
            string? configurationId = default
        ) {
            if (inputs == null || acceleration == default || acceleration.StartsWith(@"local_"))
                return local.Create(
                    tag: tag,
                    inputs: inputs,
                    acceleration: acceleration,
                    device: device,
                    clientId: clientId,
                    configurationId: configurationId
                );
            else
                return remote.Create(
                    tag: tag,
                    inputs: inputs,
                    acceleration: acceleration
                );
        }

        /// <summary>
        /// Stream a prediction.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <param name="device">Prediction device. Do not set this unless you know what you are doing.</param>
        public async IAsyncEnumerable<Prediction> Stream(
            string tag,
            Dictionary<string, object?> inputs,
            string? acceleration = default,
            IntPtr device = default
        ) {
            var stream = acceleration == default || acceleration.StartsWith("local_") ?
                local.Stream(tag, inputs, acceleration, device) :
                remote.Stream(tag, inputs, acceleration);
            await foreach (var prediction in stream)
                yield return prediction;
        }

        /// <summary>
        /// Delete a predictor that is loaded in memory.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <returns>Whether the predictor was successfully deleted from memory.</returns>
        public Task<bool> Delete(string tag) => local.Delete(tag);
        #endregion


        #region --Operations--
        private readonly LocalPredictionService local;
        private readonly RemotePredictionService remote;

        internal PredictionService(MunaClient client) {
            this.local = new LocalPredictionService(client);
            this.remote = new RemotePredictionService(client);
        }
        #endregion
    }
}