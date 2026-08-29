/*
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Muna.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Muna API client.
    /// </summary>
    public abstract class MunaClient {

        #region --Client API--
        /// <summary>
        /// Muna API URL.
        /// </summary>
        public readonly string url;

        /// <summary>
        /// Muna cache path.
        /// </summary>
        public string cachePath { get; protected set; }

        /// <summary>
        /// Make a request to a REST endpoint.
        /// </summary>
        /// <typeparam name="T">Response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <returns>Response.</returns>
        public abstract Task<T?> Request<T>(
            string method,
            string path,
            Dictionary<string, object?>? payload = default
        ) where T : class;

        /// <summary>
        /// Make a request to a REST endpoint and consume 
        /// the response as a server-sent events stream.
        /// </summary>
        /// <typeparam name="T">Response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <returns>Response stream.</returns>
        public abstract IAsyncEnumerable<T> Stream<T>(
            string method,
            string path,
            Dictionary<string, object?>? payload = default
        ) where T : class;

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">URL</param>
        public abstract Task<Stream> Download(string url);

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <param name="url">Upload URL.</param>
        /// <param name="mime">MIME type.</param>
        public abstract Task Upload(
            Stream stream,
            string url,
            string? mime = null
        );

        /// <summary>
        /// Get a cache entry.
        /// </summary>
        /// <param name="key">Cache entry key.</param>
        public abstract string? GetCacheEntry(string key);

        /// <summary>
        /// Set a cache entry.
        /// </summary>
        /// <param name="key">Cache entry key.</param>
        /// <param name="value">Cache entry value. Pass `null` to unset the key.</param>
        public abstract void SetCacheEntry(string key, string? value);
        #endregion


        #region --Operations--
        /// <summary>
        /// Muna access key.
        /// </summary>
        protected internal readonly string? accessKey;

        protected MunaClient(string url, string? accessKey) {
            this.url = url;
            this.accessKey = accessKey;
        }
        #endregion
    }

    /// <summary>
    /// Muna API error response.
    /// </summary>
    [Preserve]
    public sealed class ErrorResponse {
        public Error[] errors;
        public sealed class Error {
            public string message;
        }
    }
    
    /// <summary>
    /// Muna API exception.
    /// </summary>
    public sealed class MunaAPIException : Exception {

        public readonly int status;

        public MunaAPIException(
            string message,
            int status
        ) : base(message) => this.status = status;
    }
}