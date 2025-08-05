/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Services {

    using System.Threading.Tasks;
    using API;
    using Types;

    /// <summary>
    /// Manage predictors.
    /// </summary>
    public sealed class PredictorService {

        #region --Client API--
        /// <summary>
        /// Retrieve a predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        public async Task<Predictor?> Retrieve (string tag) {
            try {
                return await client.Request<Predictor>(
                    method: @"GET",
                    path: $"/predictors/{tag}"
                );
            } catch (FunctionAPIException ex) {
                if (ex.status == 404)
                    return null;
                throw;
            }
        }
        #endregion


        #region --Operations--
        private readonly FunctionClient client;

        internal PredictorService (FunctionClient client) => this.client = client;
        #endregion
    }
}