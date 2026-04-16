/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Services {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;    
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using API;
    using Configuration = C.Configuration;

    /// <summary>
    /// Make remote predictions.
    /// </summary>
    internal sealed class RemotePredictionService {

        #region --Client API--
        /// <summary>
        /// Create a prediction by invoking it on a cloud instance.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <returns></returns>
        public async Task<Prediction> Create(
            string tag,
            Dictionary<string, object?> inputs,
            string acceleration
        ) {
            await Configuration.InitializationTask;
            var inputMap = new Dictionary<string, RemoteValue>();
            foreach (var pair in inputs)
                inputMap[pair.Key] = await ToValue(pair.Value);
            var prediction = await client.Request<RemotePrediction>(
                method: @"POST",
                path: @"/predictions/remote",
                payload: new () {
                    [@"tag"] = tag,
                    [@"inputs"] = inputMap,
                    [@"acceleration"] = acceleration,
                    [@"clientId"] = Configuration.ClientId,
                }
            );
            return await ParseRemotePrediction(prediction);
        }

        /// <summary>
        /// Stream a prediction.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        public async IAsyncEnumerable<Prediction> Stream(
            string tag,
            Dictionary<string, object?> inputs,
            string acceleration
        ) {
            await Configuration.InitializationTask;
            var inputMap = new Dictionary<string, RemoteValue>();
            foreach (var pair in inputs)
                inputMap[pair.Key] = await ToValue(pair.Value);
            await foreach (var evt in client.Stream<RemotePredictionEvent>(
                method: @"POST",
                path: @"/predictions/remote",
                payload: new () {
                    [@"tag"] = tag,
                    [@"inputs"] = inputMap,
                    [@"acceleration"] = acceleration,
                    [@"clientId"] = Configuration.ClientId,
                    [@"stream"] = true
                }
            ))
                yield return await ParseRemotePrediction(evt.data);
        }
        #endregion


        #region --Operations--
        private readonly MunaClient client;

        internal RemotePredictionService(MunaClient client) => this.client = client;

        private async Task<RemoteValue> ToValue(object? value) => value switch { // INCOMPLETE // Image
            null              => new() { dtype = Dtype.Null },
            float           x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Float32, shape = new int[0] },
            double          x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Float64, shape = new int[0] },
            sbyte           x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Int8, shape = new int[0] },
            short           x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Int16, shape = new int[0] },
            int             x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Int32, shape = new int[0] },
            long            x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Int64, shape = new int[0] },
            byte            x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Uint8, shape = new int[0] },
            ushort          x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Uint16, shape = new int[0] },
            uint            x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Uint32, shape = new int[0] },
            ulong           x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Uint64, shape = new int[0] },
            bool            x => new() { data = await Upload(new [] { x }.ToStream()), dtype = Dtype.Bool, shape = new int[0] },
            float[]         x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Float32, shape = new [] { x.Length } },
            double[]        x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Float64, shape = new [] { x.Length } },
            sbyte[]         x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Int8, shape = new [] { x.Length } },
            short[]         x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Int16, shape = new [] { x.Length } },
            int[]           x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Int32, shape = new [] { x.Length } },
            long[]          x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Int64, shape = new [] { x.Length } },
            byte[]          x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Uint8, shape = new [] { x.Length } },
            ushort[]        x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Uint16, shape = new [] { x.Length } },
            uint[]          x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Uint32, shape = new [] { x.Length } },
            ulong[]         x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Uint64, shape = new [] { x.Length } },
            bool[]          x => new() { data = await Upload(x.ToStream()), dtype = Dtype.Bool, shape = new [] { x.Length } },
            Tensor<float>   x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Float32, shape = x.shape },
            Tensor<double>  x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Float64, shape = x.shape },
            Tensor<sbyte>   x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Int8, shape = x.shape },
            Tensor<short>   x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Int16, shape = x.shape },
            Tensor<int>     x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Int32, shape = x.shape },
            Tensor<long>    x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Int64, shape = x.shape },
            Tensor<byte>    x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Uint8, shape = x.shape },
            Tensor<ushort>  x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Uint16, shape = x.shape },
            Tensor<uint>    x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Uint32, shape = x.shape },
            Tensor<ulong>   x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Uint64, shape = x.shape },
            Tensor<bool>    x => new() { data = await Upload(x.data.ToStream()), dtype = Dtype.Bool, shape = x.shape },
            string          x => new() { data = await Upload(x.ToStream(), mime: @"text/plain"), dtype = Dtype.String },
            IList           x => new() { data = await Upload(JsonConvert.SerializeObject(x).ToStream(), mime: @"application/json"), dtype = Dtype.List },
            IDictionary     x => new() { data = await Upload(JsonConvert.SerializeObject(x).ToStream(), mime: @"application/json"), dtype = Dtype.Dict },
            Image           x => new() { data = "", dtype = Dtype.Image },
            Stream          x => new() { data = await Upload(x), dtype = Dtype.Binary },
            Enum            x => await ToValue(x.ToObject()),
            RemoteValue     x => x,
            _                 => throw new InvalidOperationException($"Failed to serialize value '{value}' of type `{value.GetType()}` because it is not supported"),
        };

        private async Task<object?> ToObject(RemoteValue value) {
            if (value.dtype == Dtype.Null)
                return null;
            using var stream = await Download(value.data!);
            return value.dtype switch {
                Dtype.Float32   => stream.ToObject<float>(value.shape!),
                Dtype.Float64   => stream.ToObject<double>(value.shape!),
                Dtype.Int8      => stream.ToObject<sbyte>(value.shape!),
                Dtype.Int16     => stream.ToObject<short>(value.shape!),
                Dtype.Int32     => stream.ToObject<int>(value.shape!),
                Dtype.Int64     => stream.ToObject<long>(value.shape!),
                Dtype.Uint8     => stream.ToObject<byte>(value.shape!),
                Dtype.Uint16    => stream.ToObject<ushort>(value.shape!),
                Dtype.Uint32    => stream.ToObject<uint>(value.shape!),
                Dtype.Uint64    => stream.ToObject<ulong>(value.shape!),
                Dtype.Bool      => stream.ToObject<bool>(value.shape!),
                Dtype.String    => new StreamReader(stream).ReadToEnd(),
                Dtype.List      => JsonConvert.DeserializeObject<JArray>(new StreamReader(stream).ReadToEnd()),
                Dtype.Dict      => JsonConvert.DeserializeObject<JObject>(new StreamReader(stream).ReadToEnd()),
                Dtype.Image     => DeserializeImageValue(stream),
                Dtype.Binary    => stream.Clone(),
                _               => throw new InvalidOperationException($"Failed to deserialize value with type {value.dtype} because it is not supported"),
            };
        }

        private Task<string> Upload(
            Stream stream,
            string? mime = @"application/octet-stream"
        ) {
            var data = Convert.ToBase64String(stream.ToArray<byte>());
            var result = $"data:{mime};base64,{data}";
            return Task.FromResult(result);
        }

        private async Task<Stream> Download(string url) {
            if (url.StartsWith(@"data:")) {
                var dataIdx = url.LastIndexOf(",") + 1;
                var b64Data = url.Substring(dataIdx);
                var data = Convert.FromBase64String(b64Data);
                return new MemoryStream(data, 0, data.Length, false, false);
            }
            return await client.Download(url);
        }

        private static Image DeserializeImageValue(Stream stream) {
            using var value = C.Value.CreateFromBinary(stream, @"image/*");
            return (Image)value.ToObject()!;
        }

        private async Task<Prediction> ParseRemotePrediction(RemotePrediction prediction) {
            object?[]? results = null;
            if (prediction?.results != null) {
                results = new object?[prediction.results.Length];
                for (var i = 0; i < results.Length; ++i)
                    results[i] = await ToObject(prediction.results[i]);
            }
            return new Prediction {
                id = prediction.id,
                tag = prediction.tag,
                created = prediction.created,
                results = results,
                latency = prediction.latency,
                error = prediction.error,
                logs = prediction.logs,
            };
        }

        [Preserve, Serializable]
        private class RemotePrediction : Prediction {
            public new RemoteValue[]? results;
        }

        [Preserve, Serializable]
        private class RemotePredictionEvent {
            [JsonProperty(@"event")]
            public string @event;
            public RemotePrediction data;
        }
        #endregion
    }
}