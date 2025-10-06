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
    public sealed class ChatCompletionService {

        #region --Client API--
        
        #endregion


        #region --Operations--
        private readonly PredictorService predictors;
        private readonly EdgePredictionService predictions;
        private readonly RemotePredictionService remotePredictions;

        internal ChatCompletionService(
            PredictorService predictors,
            EdgePredictionService predictions,
            RemotePredictionService remotePredictions
        ) {
            this.predictors = predictors;
            this.predictions = predictions;
            this.remotePredictions = remotePredictions;
        }
        #endregion
    }
}