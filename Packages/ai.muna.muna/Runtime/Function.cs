/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyCompany(@"NatML Inc.")]
[assembly: AssemblyTitle(@"Function.Runtime")]
[assembly: AssemblyVersion(Function.Function.Version)]
[assembly: AssemblyCopyright(@"Copyright © 2025 NatML Inc. All Rights Reserved.")]
[assembly: InternalsVisibleTo(@"Function.Unity")]
[assembly: InternalsVisibleTo(@"Function.Editor")]
[assembly: InternalsVisibleTo(@"Function.Tests.Editor")]
[assembly: InternalsVisibleTo(@"Function.Tests.Runtime")]

namespace Function {

    using System;
    using API;
    using Beta;
    using Services;

    /// <summary>
    /// Function client.
    /// </summary>
    public sealed class Function {

        #region --Attributes--
        /// <summary>
        /// Embed predictors at build time to avoid errors due to sandboxing restrictions.
        /// NOTE: This is required to use edge predictors on Android and iOS.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
        public sealed class EmbedAttribute : Attribute {

            internal readonly string[] tags;
            
            /// <summary>
            /// Embed predictors at build time.
            /// </summary>
            public EmbedAttribute (params string[] tags) => this.tags = tags;
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// Manage users.
        /// </summary>
        public readonly UserService Users;

        /// <summary>
        /// Manage predictors.
        /// </summary>
        public readonly PredictorService Predictors;

        /// <summary>
        /// Make predictions.
        /// </summary>
        public readonly PredictionService Predictions;

        /// <summary>
        /// Beta client for incubating features.
        /// </summary>
        public readonly BetaClient Beta;

        /// <summary>
        /// Create a Function client.
        /// </summary>
        /// <param name="accessKey">Function access key.</param>
        /// <param name="url">Function API URL.</param>
        /// <param name="cachePath">Predictor cache path.</param>
        public Function (
            string? accessKey = null,
            string? url = null
        ) : this(new DotNetClient(url ?? URL, accessKey: accessKey)) { }

        /// <summary>
        /// Create a Function client.
        /// </summary>
        /// <param name="client">Function API client.</param>
        /// <param name="cachePath">Predictor cache path.</param>
        public Function (FunctionClient client) {
            this.client = client;
            this.Users = new UserService(client);
            this.Predictors = new PredictorService(client);
            this.Predictions = new PredictionService(client);
            this.Beta = new BetaClient(client);
        }
        #endregion


        #region --Operations--
        public readonly FunctionClient client;
        public const string Version = @"0.0.42";
        internal const string URL = @"https://api.fxn.ai/v1";
        #endregion
    }

    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    internal sealed class PreserveAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class MonoPInvokeCallbackAttribute : Attribute {
        public MonoPInvokeCallbackAttribute (Type type) {}
    }
}