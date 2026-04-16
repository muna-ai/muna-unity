/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta.OpenAI {

    using Services;

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
            PredictionService predictions
        ) {
            Completions = new ChatCompletionService(predictors, predictions);
        }
        #endregion
    }
}