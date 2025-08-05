/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Beta.Services {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;    
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using API;
    using Types;
    using Configuration = C.Configuration;

    /// <summary>
    /// Make remote predictions.
    /// </summary>
    public sealed class RemotePredictionService {

        #region --Client API--
        /// <summary>
        /// Create a prediction by invoking it on a cloud instance.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <returns></returns>
        public async Task<Prediction> Create (
            string tag,
            Dictionary<string, object?> inputs,
            RemoteAcceleration acceleration = default
        ) {
            await Configuration.InitializationTask;
            var inputMap = (await Task.WhenAll(inputs.Select(async pair => (
                name: pair.Key,
                value: await ToValue(pair.Value, pair.Key)
            )))).ToDictionary(pair => pair.name, pair => pair.value);
            var prediction = (await client.Request<RemotePrediction>(
                method: @"POST",
                path: $"/predictions/remote",
                payload: new () {
                    [@"tag"] = tag,
                    [@"inputs"] = inputMap,
                    [@"acceleration"] = acceleration,
                    [@"clientId"] = Configuration.ClientId,
                }
            ))!;
            return new Prediction {
                id = prediction.id,
                tag = prediction.tag,
                created = prediction.created,
                results = prediction.results != null ?await Task.WhenAll(prediction.results.Select(ToObject)) : null,
                latency = prediction.latency,
                error = prediction.error,
                logs = prediction.logs,
            };
        }
        #endregion


        #region --Operations--
        private readonly FunctionClient client;

        internal RemotePredictionService (FunctionClient client) => this.client = client;

        private async Task<Value> ToValue ( // INCOMPLETE // Image
            object? value,
            string name,
            int maxDataUrlSize = 4 * 1024 * 1024
        ) => value switch {
            null              => new Value { type = Dtype.Null },
            float           x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Float32, shape = new int[0] },
            double          x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Float64, shape = new int[0] },
            sbyte           x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int8, shape = new int[0] },
            short           x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int16, shape = new int[0] },
            int             x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int32, shape = new int[0] },
            long            x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int64, shape = new int[0] },
            byte            x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint8, shape = new int[0] },
            ushort          x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint16, shape = new int[0] },
            uint            x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint32, shape = new int[0] },
            ulong           x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint64, shape = new int[0] },
            bool            x => new Value { data = await Upload(new [] { x }.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Bool, shape = new int[0] },
            float[]         x => new Value { data = await Upload(x.ToStream(), name,  maxDataUrlSize: maxDataUrlSize), type = Dtype.Float32, shape = new [] { x.Length } },
            double[]        x => new Value { data = await Upload(x.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Float64, shape = new [] { x.Length } },
            sbyte[]         x => new Value { data = await Upload(x.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int8, shape = new [] { x.Length } },
            short[]         x => new Value { data = await Upload(x.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int16, shape = new [] { x.Length } },
            int[]           x => new Value { data = await Upload(x.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int32, shape = new [] { x.Length } },
            long[]          x => new Value { data = await Upload(x.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int64, shape = new [] { x.Length } },
            byte[]          x => new Value { data = await Upload(x.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint8, shape = new [] { x.Length } },
            ushort[]        x => new Value { data = await Upload(x.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint16, shape = new [] { x.Length } },
            uint[]          x => new Value { data = await Upload(x.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint32, shape = new [] { x.Length } },
            ulong[]         x => new Value { data = await Upload(x.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint64, shape = new [] { x.Length } },
            bool[]          x => new Value { data = await Upload(x.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Bool, shape = new [] { x.Length } },
            Tensor<float>   x => new Value { data = await Upload(x.data.ToStream(),name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Float32, shape = x.shape },
            Tensor<double>  x => new Value { data = await Upload(x.data.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Float64, shape = x.shape },
            Tensor<sbyte>   x => new Value { data = await Upload(x.data.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int8, shape = x.shape },
            Tensor<short>   x => new Value { data = await Upload(x.data.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int16, shape = x.shape },
            Tensor<int>     x => new Value { data = await Upload(x.data.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int32, shape = x.shape },
            Tensor<long>    x => new Value { data = await Upload(x.data.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Int64, shape = x.shape },
            Tensor<byte>    x => new Value { data = await Upload(x.data.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint8, shape = x.shape },
            Tensor<ushort>  x => new Value { data = await Upload(x.data.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint16, shape = x.shape },
            Tensor<uint>    x => new Value { data = await Upload(x.data.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint32, shape = x.shape },
            Tensor<ulong>   x => new Value { data = await Upload(x.data.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Uint64, shape = x.shape },
            Tensor<bool>    x => new Value { data = await Upload(x.data.ToStream(), name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Bool, shape = x.shape },
            string          x => new Value { data = await Upload(x.ToStream(), name, mime: @"text/plain", maxDataUrlSize: maxDataUrlSize), type = Dtype.String },
            IList           x => new Value { data = await Upload(JsonConvert.SerializeObject(x).ToStream(), name, mime: @"application/json", maxDataUrlSize: maxDataUrlSize), type = Dtype.List },
            IDictionary     x => new Value { data = await Upload(JsonConvert.SerializeObject(x).ToStream(), name, mime: @"application/json", maxDataUrlSize: maxDataUrlSize), type = Dtype.Dict },
            Image           x => new Value { data = "", type = Dtype.Image },
            Stream          x => new Value { data = await Upload(x, name, maxDataUrlSize: maxDataUrlSize), type = Dtype.Binary },
            Enum            x => await ToValue(x.ToObject(), name, maxDataUrlSize: maxDataUrlSize),
            _                 => throw new InvalidOperationException($"Failed to serialize value '{value}' of type `{value.GetType()}` because it is not supported"),
        };

        private async Task<object?> ToObject (Value value) { // INCOMPLETE // Image
            if (value.type == Dtype.Null)
                return null;
            using var stream = await Download(value.data!);
            return value.type switch {
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
                Dtype.Image     => default,
                Dtype.Binary    => stream.Clone(),
                _               => throw new InvalidOperationException($"Failed to deserialize value with type {value.type} because it is not supported"),
            };
        }

        private async Task<string> Upload (
            Stream stream,
            string name,
            string? mime = @"application/octet-stream",
            int maxDataUrlSize = 4 * 1024 * 1024
        ) {
            if (stream.Length <= maxDataUrlSize) {
                var data = Convert.ToBase64String(stream.ToArray<byte>());
                var result = $"data:{mime};base64,{data}";
                return result;
            }
            var value = await client.Request<CreateValueResponse>(
                method: @"POST",
                path: "/values",
                payload: new () { [@"name"] = name }
            );
            await client.Upload(stream, value!.uploadUrl!, mime: mime);
            return value.downloadUrl!;
        }

        private async Task<Stream> Download (string url) {
            if (url.StartsWith(@"data:")) {
                var dataIdx = url.LastIndexOf(",") + 1;
                var b64Data = url.Substring(dataIdx);
                var data = Convert.FromBase64String(b64Data);
                return new MemoryStream(data, 0, data.Length, false, false);
            }
            return await client.Download(url);
        }

        [Preserve, Serializable]
        private class CreateValueResponse {
            public string? uploadUrl;
            public string? downloadUrl;
        }

        [Preserve, Serializable]
        private class RemotePrediction : Prediction {
            public new Value[]? results;
        }
        #endregion
    }
}