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
    /// Mock OpenAI client.
    /// </summary>
    public sealed class OpenAIClient {

        #region --Client API--
        /// <summary>
        /// Create chat conversations.
        /// </summary>
        public readonly ChatService Chat;

        /// <summary>
        /// Create embedding vectors.
        /// </summary>
        public readonly EmbeddingService Embeddings;

        /// <summary>
        /// Create speech and transcriptions.
        /// </summary>
        public readonly AudioService Audio;
        #endregion


        #region --Operations--

        internal OpenAIClient(
            PredictorService predictors,
            EdgePredictionService predictions,
            RemotePredictionService remotePredictions
        ) {
            Chat = new ChatService(predictors, predictions, remotePredictions);
            Embeddings = new EmbeddingService(predictors, predictions, remotePredictions);
            Audio = new AudioService(predictors, predictions, remotePredictions);
        }
        #endregion
    }
}