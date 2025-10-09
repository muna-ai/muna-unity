/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta.OpenAI {

    using Services;
    using PredictorService = global::Muna.Services.PredictorService;
    using EdgePredictionService = global::Muna.Services.PredictionService;

    /// <summary>
    /// Create speech and transcriptions.
    /// </summary>
    public sealed class AudioService {

        #region --Client API--
        /// <summary>
        /// Create speech.
        /// </summary>
        public readonly SpeechService Speech;
        #endregion


        #region --Operations--

        internal AudioService(
            PredictorService predictors,
            EdgePredictionService predictions,
            RemotePredictionService remotePredictions
        ) {
            Speech = new SpeechService(predictors, predictions, remotePredictions);
        }
        #endregion
    }
}