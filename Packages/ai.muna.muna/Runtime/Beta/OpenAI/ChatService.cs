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
    /// Create chat conversations.
    /// </summary>
    public sealed class ChatService {

        #region --Client API--
        /// <summary>
        /// Create completions.
        /// </summary>
        public readonly ChatCompletionService Completions;
        #endregion


        #region --Operations--

        internal ChatService(
            PredictorService predictors,
            EdgePredictionService predictions,
            RemotePredictionService remotePredictions
        ) {
            Completions = new ChatCompletionService(predictors, predictions, remotePredictions);
        }
        #endregion
    }
}