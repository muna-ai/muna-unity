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
    /// Mock OpenAI client.
    /// </summary>
    public sealed class OpenAIClient {

        #region --Client API--
        /// <summary>
        /// Create chat conversations.
        /// </summary>
        public readonly ChatService chat;

        /// <summary>
        /// Create embedding vectors.
        /// </summary>
        public readonly EmbeddingService embeddings;

        /// <summary>
        /// Create speech and transcriptions.
        /// </summary>
        public readonly AudioService audio;
        #endregion


        #region --Operations--

        internal OpenAIClient(
            PredictorService predictors,
            EdgePredictionService predictions,
            RemotePredictionService remotePredictions
        ) {
            this.chat = new ChatService(predictors, predictions, remotePredictions);
            this.embeddings = new EmbeddingService(predictors, predictions, remotePredictions);
            this.audio = new AudioService(predictors, predictions, remotePredictions);
        }
        #endregion
    }
}