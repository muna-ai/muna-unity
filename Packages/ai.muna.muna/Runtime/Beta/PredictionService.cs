/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta.Services {

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

        internal PredictionService (MunaClient client) {
            this.Remote = new RemotePredictionService(client);
        }
        #endregion
    }
}