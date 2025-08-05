/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Beta.Services {

    using API;

    /// <summary>
    /// Make predictions.
    /// </summary>
    public sealed class PredictionService {

        #region --Client API--
        /// <summary>
        /// Make remote predictions.
        /// </summary>
        public readonly RemotePredictionService Remote;
        #endregion


        #region --Operations--

        internal PredictionService (FunctionClient client) {
            this.Remote = new RemotePredictionService(client);
        }
        #endregion
    }
}