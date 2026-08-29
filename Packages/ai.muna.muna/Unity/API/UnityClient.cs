/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Muna API client for Unity Engine.
    /// This uses Unity APIs for performing web requests.
    /// </summary>
    internal class UnityClient : MunaClient {

        #region --Client API--
        /// <summary>
        /// Create the client.
        /// </summary>
        /// <param name="url">Muna API URL.</param>
        /// <param name="accessKey">Muna access key.</param>
        public UnityClient(
            string url,
            string? accessKey
        ) : base(url.TrimEnd('/'), accessKey) {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            cachePath = Application.isEditor ?
                Path.Combine(homeDir, @".fxn") :
                Path.Combine(Application.persistentDataPath, @"fxn");
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
            // Create client
            using var client = new UnityWebRequest($"{this.url}{path}", method) {
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
                timeout = Timeout,
            };
            // Add auth header
            if (!string.IsNullOrEmpty(accessKey))
                client.SetRequestHeader(@"Authorization", $"Bearer {accessKey}");
            // Add payload
            if (payload != null) {
                var serializationSettings = new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore
                };
                var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
                client.SetRequestHeader(@"Content-Type",  @"application/json");
                client.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadStr));
            }
            // Request
            client.SendWebRequest();
            while (!client.isDone)
                await Task.Yield();
            // Check error
            var responseStr = client.downloadHandler.text;
            if (client.responseCode == 0)
                throw new MunaAPIException(
                    @"Failed to get response from server. Check that you have an internet connection.",
                    (int)client.responseCode
                );
            if (client.responseCode >= 400) {
                var errorPayload = JsonConvert.DeserializeObject<ErrorResponse>(responseStr);
                var error = errorPayload?.errors?[0]?.message ?? @"An unknown error occurred";
                throw new MunaAPIException(error, (int)client.responseCode);
            }
            // Return
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
            // Create request
            var handler = new SSEDownloadHandler();
            using var client = new UnityWebRequest($"{this.url}{path}", method) {
                downloadHandler = handler,
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
                timeout = Timeout
            };
            if (!string.IsNullOrEmpty(accessKey))
                client.SetRequestHeader(@"Authorization", $"Bearer {accessKey}");
            client.SetRequestHeader(@"Accept", @"text/event-stream");
            if (payload != null) {
                var serializationSettings = new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore
                };
                var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
                client.SetRequestHeader(@"Content-Type", @"application/json");
                client.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadStr));
            }
            // Send
            client.SendWebRequest();
            while (client.responseCode == 0 && !client.isDone)
                await Task.Yield();
            // Check error
            if (client.responseCode == 0)
                throw new MunaAPIException(
                    @"Failed to get response from server. Check that you have an internet connection.",
                    (int)client.responseCode
                );
            if (client.responseCode >= 400) {
                while (!client.isDone)
                    await Task.Yield();
                var sb = new StringBuilder();
                while (handler.lines.Count > 0)
                    sb.AppendLine(handler.lines.Dequeue());
                var errorPayload = JsonConvert.DeserializeObject<ErrorResponse>(sb.ToString());
                var error = errorPayload?.errors?[0]?.message ?? @"An unknown error occurred";
                throw new MunaAPIException(error, (int)client.responseCode);
            }
            // Parse SSE stream
            string? eventName = null;
            var data = string.Empty;
            while (true) {
                while (handler.lines.Count > 0) {
                    var line = handler.lines.Dequeue().Trim();
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
                if (client.isDone)
                    break;
                await Task.Yield();
            }
            if (eventName != null || !string.IsNullOrEmpty(data))
                yield return ParseSSEEvent<T>(eventName!, data);
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">URL</param>
        public override async Task<Stream> Download(string url) {
            using var request = UnityWebRequest.Get(url);
            request.timeout = Timeout;
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                throw new InvalidOperationException(request.error);
            var data = request.downloadHandler.data;
            var stream = new MemoryStream(data, 0, data.Length, false, false);
            return stream;
        }

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
            using var client = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT) {
                uploadHandler = new UploadHandlerRaw(stream.ToArray<byte>()),
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
                timeout = Timeout,
            };
            client.SetRequestHeader(@"Content-Type", mime ?? @"application/octet-stream");
            client.SendWebRequest();
            while (!client.isDone)
                await Task.Yield();
            if (client.error != null)
                throw new InvalidOperationException($"Failed to upload stream with error: {client.error}");
        }

        /// <summary>
        /// Get a cache entry.
        /// </summary>
        /// <param name="key">Cache entry key.</param>
        public override string? GetCacheEntry(string key) => PlayerPrefs.HasKey(key) ?
            PlayerPrefs.GetString(key) :
            null;

        /// <summary>
        /// Set a cache entry.
        /// </summary>
        /// <param name="key">Cache entry key.</param>
        /// <param name="value">Cache entry value. Pass `null` to unset the key.</param>
        public override void SetCacheEntry(string key, string? value) {
            if (value != null)
                PlayerPrefs.SetString(key, value);
            else
                PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
        #endregion


        #region --Operations--
        private const int Timeout = 300; // seconds

        private static T ParseSSEEvent<T> (string? eventName, string data) where T : class {
            var payload = new JObject {
                [@"event"] = eventName,
                [@"data"] = JToken.Parse(data)
            };
            return payload.ToObject<T>()!;
        }
        #endregion


        #region --Download Handler--

        private class SSEDownloadHandler : DownloadHandlerScript {
            public readonly Queue<string> lines = new();
            private string buffer = string.Empty;

            protected override bool ReceiveData(byte[] data, int dataLength) {
                buffer += Encoding.UTF8.GetString(data, 0, dataLength);
                while (true) {
                    var idx = buffer.IndexOf('\n');
                    if (idx < 0)
                        break;
                    lines.Enqueue(buffer.Substring(0, idx));
                    buffer = buffer.Substring(idx + 1);
                }
                return true;
            }

            protected override void CompleteContent() {
                if (buffer.Length > 0) {
                    lines.Enqueue(buffer);
                    buffer = string.Empty;
                }
            }
        }
        #endregion
    }
}