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
    /// Manage users.
    /// </summary>
    public sealed class UserService {

        #region --Client API--
        /// <summary>
        /// Retrieve the current user.
        /// </summary>
        public async Task<User?> Retrieve () {
            try {
                return await client.Request<User>(
                    method: @"GET",
                    path: @"/users"
                );
            } catch (FunctionAPIException ex) {
                if (ex.status == 401)
                    return null;
                throw;
            }
        }
        #endregion


        #region --Operations--
        private readonly FunctionClient client;

        internal UserService (FunctionClient client) => this.client = client;
        #endregion
    }
}