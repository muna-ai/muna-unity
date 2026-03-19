/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
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

        /// <summary>
        /// Create transcriptions.
        /// </summary>
        public readonly TranscriptionService Transcriptions;
        #endregion


        #region --Operations--

        internal AudioService(
            PredictorService predictors,
            EdgePredictionService predictions,
            RemotePredictionService remotePredictions
        ) {
            Speech = new SpeechService(predictors, predictions, remotePredictions);
            Transcriptions = new TranscriptionService(predictors, predictions, remotePredictions);
        }
        #endregion
    }
}