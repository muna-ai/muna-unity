/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta {

    using API;
    using Services;
    using PredictorService = global::Muna.Services.PredictorService;
    using EdgePredictionService = global::Muna.Services.PredictionService;

    /// <summary>
    /// Client for incubating features.
    /// </summary>
    public sealed class BetaClient {

        #region --Client API--
        /// <summary>
        /// Make predictions.
        /// </summary>
        public readonly PredictionService Predictions;
        #endregion


        #region --Operations--

        internal BetaClient(
            MunaClient client,
            PredictorService predictors,
            EdgePredictionService predictions
        ) {
            this.Predictions = new PredictionService(client);
        }
        #endregion
    }
}