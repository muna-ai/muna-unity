/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine.Networking;
    using Newtonsoft.Json;

    /// <summary>
    /// Function API client for Unity Engine.
    /// This uses Unity APIs for performing web requests.
    /// </summary>
    internal class UnityClient : FunctionClient {

        #region --Client API--
        /// <summary>
        /// Create the client.
        /// </summary>
        /// <param name="url">Function API URL.</param>
        /// <param name="accessKey">Function access key.</param>
        /// <param name="cache">Prediction cache.</param>
        public UnityClient (
            string url,
            string? accessKey
        ) : base(url.TrimEnd('/'), accessKey) { }

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
            // Create client
            using var client = new UnityWebRequest($"{this.url}{path}", method) {
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
                timeout = 20,
            };
            // Add headers
            if (!string.IsNullOrEmpty(accessKey))
                client.SetRequestHeader(@"Authorization", $"Bearer {accessKey}");
            if (headers != null)
                foreach (var header in headers)
                    client.SetRequestHeader(header.Key, header.Value);
            // Add payload
            if (payload != null) {
                var serializationSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
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
                throw new FunctionAPIException(
                    @"Failed to get response from server. Check that you have an internet connection.",
                    (int)client.responseCode
                );
            if (client.responseCode >= 400) {
                var errorPayload = JsonConvert.DeserializeObject<ErrorResponse>(responseStr);
                var error = errorPayload?.errors?[0]?.message ?? @"An unknown error occurred";
                throw new FunctionAPIException(error, (int)client.responseCode);
            }
            // Return
            return JsonConvert.DeserializeObject<T>(responseStr)!;
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">URL</param>
        public override async Task<Stream> Download (string url) {
            using var request = UnityWebRequest.Get(url);
            request.timeout = 20;
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
        public override async Task Upload (Stream stream, string url, string? mime = null) {
            using var client = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT) {
                uploadHandler = new UploadHandlerRaw(ToArray(stream)),
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
                timeout = 20,
            };
            client.SetRequestHeader(@"Content-Type", mime ?? @"application/octet-stream");
            client.SendWebRequest();
            while (!client.isDone)
                await Task.Yield();
            if (client.error != null)
                throw new InvalidOperationException($"Failed to upload stream with error: {client.error}");
        }
        #endregion


        #region --Operations--

        private static byte[] ToArray (Stream stream) {
            if (stream is MemoryStream memoryStream)
                return memoryStream.ToArray();
            using var dstStream = new MemoryStream();
            stream.CopyTo(dstStream);
            return dstStream.ToArray();
        }
        #endregion
    }
}