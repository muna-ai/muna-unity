/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Beta {

    using API;
    using Services;

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

        internal BetaClient (FunctionClient client) {
            this.Predictions = new PredictionService(client);
        }
        #endregion
    }
}