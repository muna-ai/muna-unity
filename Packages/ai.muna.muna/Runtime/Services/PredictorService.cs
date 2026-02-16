/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Services {

    using System.Threading.Tasks;
    using API;

    /// <summary>
    /// Manage predictors.
    /// </summary>
    public sealed class PredictorService {

        #region --Client API--
        /// <summary>
        /// Retrieve a predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        public async Task<Predictor?> Retrieve(string tag) {
            try {
                return await client.Request<Predictor>(
                    method: @"GET",
                    path: $"/predictors/{tag}"
                );
            } catch (MunaAPIException ex) {
                if (ex.status == 404)
                    return null;
                throw;
            }
        }
        #endregion


        #region --Operations--
        private readonly MunaClient client;

        internal PredictorService(MunaClient client) => this.client = client;
        #endregion
    }
}