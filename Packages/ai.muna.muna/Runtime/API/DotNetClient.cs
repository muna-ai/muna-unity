/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Function API client for .NET.
    /// </summary>
    public sealed class DotNetClient : FunctionClient {

        #region --Client API--
        /// <summary>
        /// Create the .NET Function API client.
        /// </summary>
        /// <param name="url">Function API URL.</param>
        /// <param name="accessKey">Function access key.</param>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="deviceId">Device model identifier.</param>
        /// <param name="cachePath">Prediction resource cache path.</param>
        public DotNetClient (
            string url,
            string? accessKey = default
        ) : base(url.TrimEnd('/'), accessKey) {
            client = new();
            var ua = new ProductInfoHeaderValue(@"FunctionDotNet", Function.Version);
            client.DefaultRequestHeaders.UserAgent.Add(ua);
        }

        /// <summary>
        /// Make a request to a REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <param name="headers">Request headers.</param>
        /// <returns>Deserialized response.</returns>
        public override async Task<T?> Request<T> (
            string method,
            string path,
            Dictionary<string, object?>? payload = default,
            Dictionary<string, string>? headers = default
        ) where T : class {
            using var message = new HttpRequestMessage(new HttpMethod(method), $"{url}{path}");       
            if (!string.IsNullOrEmpty(accessKey))
                message.Headers.Authorization = new AuthenticationHeaderValue(@"Bearer", accessKey);
            if (headers != null)
                foreach (var header in headers)
                    message.Headers.Add(header.Key, header.Value);
            if (payload != null) {
                var serializationSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
                message.Content = new StringContent(payloadStr, Encoding.UTF8, @"application/json");
            }
            using var response = await client.SendAsync(message);
            var responseStr = await response.Content.ReadAsStringAsync();
            if ((int)response.StatusCode >= 400) {
                var errorPayload = JsonConvert.DeserializeObject<ErrorResponse>(responseStr);
                var error = errorPayload?.errors?[0]?.message ?? @"An unknown error occurred";
                throw new FunctionAPIException(error, (int)response.StatusCode);
            }
            return JsonConvert.DeserializeObject<T>(responseStr)!;
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">Data URL.</param>
        public override Task<Stream> Download (string url) => client.GetStreamAsync(url);

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <param name="url">Upload URL.</param>
        /// <param name="mime">MIME type.</param>
        public override async Task Upload (Stream stream, string url, string? mime = null) {
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(mime ?? @"application/octet-stream");
            using var response = await client.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        #endregion


        #region --Operations--
        private readonly HttpClient client;
        #endregion
    }
}