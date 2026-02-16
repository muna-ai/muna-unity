/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.API {

    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Muna API client for .NET.
    /// </summary>
    public sealed class DotNetClient : MunaClient {

        #region --Client API--
        /// <summary>
        /// Create the .NET Muna API client.
        /// </summary>
        /// <param name="url">Muna API URL.</param>
        /// <param name="accessKey">Muna access key.</param>
        public DotNetClient(
            string url,
            string? accessKey = default
        ) : base(url.TrimEnd('/'), accessKey) {
            client = new();
            var ua = new ProductInfoHeaderValue(@"MunaDotNet", Muna.Version);
            client.DefaultRequestHeaders.UserAgent.Add(ua);
        }

        /// <summary>
        /// Make a request to a REST endpoint.
        /// </summary>
        /// <typeparam name="T">Response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <returns>Response.</returns>
        public override async Task<T?> Request<T>(
            string method,
            string path,
            Dictionary<string, object?>? payload = default
        ) where T : class {
            using var response = await SendAsync(method, path, payload);
            var responseStr = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseStr)!;
        }

        /// <summary>
        /// Make a request to a REST endpoint and consume 
        /// the response as a server-sent events stream.
        /// </summary>
        /// <typeparam name="T">Response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <returns>Response stream.</returns>
        public override async IAsyncEnumerable<T> Stream<T>(
            string method,
            string path,
            Dictionary<string, object?>? payload = default
        ) where T : class {
            using var response = await SendAsync(
                method, path, payload,
                HttpCompletionOption.ResponseHeadersRead
            );
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            string? eventName = null;
            var data = string.Empty;
            while (true) {
                var line = await reader.ReadLineAsync();
                if (line == null)
                    break;
                line = line.Trim();
                if (!string.IsNullOrEmpty(line)) {
                    if (line.StartsWith(@"event:"))
                        eventName = line.Substring(@"event:".Length).Trim();
                    else if (line.StartsWith(@"data:")) {
                        var lineData = line.Substring(@"data:".Length).Trim();
                        data = string.IsNullOrEmpty(data) ? lineData : $"{data}\n{lineData}";
                    }
                    continue;
                }
                if (eventName != null)
                    yield return ParseSSEEvent<T>(eventName, data);
                eventName = null;
                data = string.Empty;
            }
            if (eventName != null || !string.IsNullOrEmpty(data))
                yield return ParseSSEEvent<T>(eventName!, data);
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">Data URL.</param>
        public override Task<Stream> Download(string url) => client.GetStreamAsync(url);

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <param name="url">Upload URL.</param>
        /// <param name="mime">MIME type.</param>
        public override async Task Upload(
            Stream stream,
            string url,
            string? mime = null
        ) {
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(mime ?? @"application/octet-stream");
            using var response = await client.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        #endregion


        #region --Operations--
        private readonly HttpClient client;

        private async Task<HttpResponseMessage> SendAsync(
            string method,
            string path,
            Dictionary<string, object?>? payload,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead
        ) {
            using var message = new HttpRequestMessage(new HttpMethod(method), $"{url}{path}");
            if (!string.IsNullOrEmpty(accessKey))
                message.Headers.Authorization = new AuthenticationHeaderValue(@"Bearer", accessKey);
            if (completionOption == HttpCompletionOption.ResponseHeadersRead)
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(@"text/event-stream"));
            if (payload != null) {
                var serializationSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
                message.Content = new StringContent(payloadStr, Encoding.UTF8, @"application/json");
            }
            var response = await client.SendAsync(message, completionOption);
            if ((int)response.StatusCode >= 400) {
                using (response) {
                    var errorStr = await response.Content.ReadAsStringAsync();
                    var errorPayload = JsonConvert.DeserializeObject<ErrorResponse>(errorStr);
                    var error = errorPayload?.errors?[0]?.message ?? @"An unknown error occurred";
                    throw new MunaAPIException(error, (int)response.StatusCode);
                }
            }
            return response;
        }

        private static T ParseSSEEvent<T>(string? eventName, string data) where T : class {
            var payload = new JObject {
                [@"event"] = eventName,
                [@"data"] = JToken.Parse(data)
            };
            return payload.ToObject<T>()!;
        }
        #endregion
    }
}