/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta {

    using API;
    using OpenAI;
    using Services;

    /// <summary>
    /// Client for incubating features.
    /// </summary>
    public sealed class BetaClient {

        #region --Client API--
        /// <summary>
        /// OpenAI client.
        /// </summary>
        public readonly OpenAIClient OpenAI;
        #endregion


        #region --Operations--

        internal BetaClient(
            MunaClient client,
            PredictorService predictors,
            PredictionService predictions
        ) {
            this.OpenAI = new OpenAIClient(predictors, predictions);
        }
        #endregion
    }
}