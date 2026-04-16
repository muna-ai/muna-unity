/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta.OpenAI {

    using Services;

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
            PredictionService predictions
        ) {
            Speech = new SpeechService(predictors, predictions);
            Transcriptions = new TranscriptionService(predictors, predictions);
        }
        #endregion
    }
}