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
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using API;
    using Configuration = C.Configuration;
    using Value = C.Value;
    using ValueMap = C.ValueMap;

    /// <summary>
    /// Make predictions.
    /// </summary>
    public sealed class PredictionService {

        #region --Client API--
        /// <summary>
        /// Create a prediction.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <param name="device">Prediction device. Do not set this unless you know what you are doing.</param>
        /// <param name="clientId">Muna client identifier. Specify this to override the current client identifier.</param>
        /// <param name="configurationId">Configuration identifier. Specify this to override the current client configuration token.</param>
        public Task<Prediction> Create(
            string tag,
            Dictionary<string, object?>? inputs = default,
            string? acceleration = default,
            IntPtr device = default,
            string? clientId = default,
            string? configurationId = default
        ) {
            if (inputs == null)
                return CreateRawPrediction(
                    tag,
                    clientId: clientId,
                    configurationId: configurationId
                );
            if (inputs.Count == 0 || acceleration == default || acceleration.StartsWith(@"local_"))
                return CreateLocalPrediction(
                    tag,
                    inputs,
                    acceleration: acceleration,
                    device: device,
                    clientId: clientId,
                    configurationId: configurationId
                );
            return CreateRemotePrediction(
                tag,
                inputs,
                acceleration: acceleration
            );
        }

        /// <summary>
        /// Stream a prediction.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <param name="device">Prediction device. Do not set this unless you know what you are doing.</param>
        public async IAsyncEnumerable<Prediction> Stream(
            string tag,
            Dictionary<string, object?> inputs,
            string? acceleration = default,
            IntPtr device = default
        ) {
            var stream = acceleration == default || acceleration.StartsWith("local_") ?
                StreamLocalPrediction(tag, inputs, acceleration, device) :
                StreamRemotePrediction(tag, inputs, acceleration);
            await foreach (var prediction in stream)
                yield return prediction;
        }

        /// <summary>
        /// Delete a predictor that is loaded in memory.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <returns>Whether the predictor was successfully deleted from memory.</returns>
        public async Task<bool> Delete(string tag) {
            await Configuration.InitializationTask;
            if (!cache.TryGetValue(tag, out var predictor))
                return false;
            predictor.Dispose();
            cache.Remove(tag);
            return true;
        }
        #endregion


        #region --Operations--
        private readonly MunaClient client;
        private readonly Dictionary<string, C.Predictor> cache = new();

        internal PredictionService(MunaClient client) => this.client = client;

        private Task<Prediction> CreateRawPrediction(
            string tag,
            string? clientId = default,
            string? configurationId = default
        ) => client.Request<Prediction>(
            method: @"POST",
            path: $"/predictions",
            payload: new () {
                [@"tag"] = tag,
                [@"clientId"] = clientId ?? Configuration.ClientId,
                [@"configurationId"] = configurationId ?? Configuration.ConfigurationId,
            }
        )!;

        private async Task<Prediction> CreateLocalPrediction(
            string tag,
            Dictionary<string, object?> inputs,
            string? acceleration = default,
            IntPtr device = default,
            string? clientId = default,
            string? configurationId = default
        ) {
            await Configuration.InitializationTask;
            if (inputs.Count == 0) {
                var pred = await CreateRawPrediction(
                    tag,
                    clientId: clientId,
                    configurationId: configurationId
                );
                await CreateCachedPrediction(pred);
                return pred;
            }
            var predictor = await GetPredictor(
                tag,
                acceleration: acceleration,
                device: device,
                clientId: clientId,
                configurationId: configurationId
            );
            using var inputMap = ToValueMap(inputs);
            using var prediction = predictor.CreatePrediction(inputMap);
            return ToPrediction(tag, prediction);
        }

        private async Task<Prediction> CreateRemotePrediction(
            string tag,
            Dictionary<string, object?> inputs,
            string acceleration
        ) {
            await Configuration.InitializationTask;
            var inputMap = new Dictionary<string, RemoteValue>();
            foreach (var pair in inputs)
                inputMap[pair.Key] = await ToRemoteValue(pair.Value);
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
            return await ParseRemotePrediction(prediction!);
        }

        private async IAsyncEnumerable<Prediction> StreamLocalPrediction(
            string tag,
            Dictionary<string, object?> inputs,
            string? acceleration = default,
            IntPtr device = default
        ) {
            await Configuration.InitializationTask;
            var predictor = await GetPredictor(tag, acceleration, device);
            using var inputMap = ToValueMap(inputs);
            using var stream = predictor.StreamPrediction(inputMap);
            C.Prediction? prediction = null;
            while ((prediction = stream.ReadNext()) != null)
                using (prediction)
                    yield return ToPrediction(tag, prediction);   
        }

        private async IAsyncEnumerable<Prediction> StreamRemotePrediction(
            string tag,
            Dictionary<string, object?> inputs,
            string acceleration
        ) {
            await Configuration.InitializationTask;
            var inputMap = new Dictionary<string, RemoteValue>();
            foreach (var pair in inputs)
                inputMap[pair.Key] = await ToRemoteValue(pair.Value);
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

        private async Task<C.Predictor> GetPredictor(
            string tag,
            string? acceleration = default,
            IntPtr device = default,
            string? clientId = default,
            string? configurationId = default
        ) {
            if (cache.TryGetValue(tag, out var p))
                return p;
            var prediction = await CreateRawPrediction(
                tag,
                clientId: clientId,
                configurationId: configurationId
            );
            prediction = await CreateCachedPrediction(prediction);
            using var configuration = new Configuration() {
                tag = prediction.tag,
                token = prediction.configuration!,
                acceleration = ToAcceleration(acceleration),
                device = device
            };
            foreach (var resource in prediction.resources!)
                await configuration.AddResource(resource.type, resource.url);
            foreach (var entry in ParsePreloadClaim(prediction.configuration!)) {
                var preload = await Create(
                    entry.tag,
                    inputs: new() { [@"_"] = null },
                    acceleration: entry.acceleration
                );
                if (!string.IsNullOrEmpty(preload.error))
                    throw new InvalidOperationException($"Failed to preload {entry.tag}: {preload.error}");
                if (
                    prediction.results == null      ||
                    prediction.results.Length < 1   ||
                    !(prediction.results[0] is string metadata)
                )
                    throw new InvalidOperationException($"Failed to preload {entry.tag} because it did not return a string as its first result");
                configuration.SetMetadata(entry.metadata, metadata);
            }
            var predictor = new C.Predictor(configuration);
            cache.Add(tag, predictor);
            return predictor;
        }

        private async Task<Prediction> CreateCachedPrediction(Prediction prediction) {
            var resources = await Task.WhenAll(prediction.resources.Select(Download));
            return new() {
                id = prediction.id,
                tag = prediction.tag,
                created = prediction.created,
                resources = resources,
                configuration = prediction.configuration,
            };
        }

        private async Task<Prediction> ParseRemotePrediction(RemotePrediction prediction) {
            object?[]? results = null;
            if (prediction.results != null) {
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

        public static PreloadEntry[] ParsePreloadClaim(string configurationToken) {
            try {
                var parts = configurationToken.Split('.');
                if (parts.Length < 2)
                    return Array.Empty<PreloadEntry>();
                var payload = parts[1];
                payload = payload.Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                var claims = JObject.Parse(json);
                if (!(claims["preload"] is JArray preload))
                    return Array.Empty<PreloadEntry>();
                var settings = new JsonSerializerSettings {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var serializer = JsonSerializer.CreateDefault(settings);
                var entries = preload.ToObject<PreloadEntry[]>(serializer)!;
                return entries;
            } catch (Exception) {
                return Array.Empty<PreloadEntry>();
            }
        }

        internal static Value ToValue(object? value) => value switch {
            Value           x => x,
            IntPtr          x => new Value(x),
            float           x => Value.CreateArray(x),
            double          x => Value.CreateArray(x),
            sbyte           x => Value.CreateArray(x),
            short           x => Value.CreateArray(x),
            int             x => Value.CreateArray(x),
            long            x => Value.CreateArray(x),
            byte            x => Value.CreateArray(x),
            ushort          x => Value.CreateArray(x),
            uint            x => Value.CreateArray(x),
            ulong           x => Value.CreateArray(x),
            bool            x => Value.CreateArray(x),
            float[]         x => Value.CreateArray(x),
            double[]        x => Value.CreateArray(x),
            sbyte[]         x => Value.CreateArray(x),
            short[]         x => Value.CreateArray(x),
            int[]           x => Value.CreateArray(x),
            long[]          x => Value.CreateArray(x),
            byte[]          x => Value.CreateArray(x),
            ushort[]        x => Value.CreateArray(x),
            uint[]          x => Value.CreateArray(x),
            ulong[]         x => Value.CreateArray(x),
            bool[]          x => Value.CreateArray(x),
            Tensor<float>   x => Value.CreateArray(x),
            Tensor<double>  x => Value.CreateArray(x),
            Tensor<sbyte>   x => Value.CreateArray(x),
            Tensor<short>   x => Value.CreateArray(x),
            Tensor<int>     x => Value.CreateArray(x),
            Tensor<long>    x => Value.CreateArray(x),
            Tensor<byte>    x => Value.CreateArray(x),
            Tensor<ushort>  x => Value.CreateArray(x),
            Tensor<uint>    x => Value.CreateArray(x),
            Tensor<ulong>   x => Value.CreateArray(x),
            Tensor<bool>    x => Value.CreateArray(x),
            string          x => Value.CreateString(x),
            Enum            x => ToValue(x.SerializeEnum()),
            IList           x => Value.CreateList(Json.From(x)),
            IDictionary     x => Value.CreateDict(Json.From(x)),
            Json            x when x.IsArray => Value.CreateList(x),
            Json            x when x.IsObject => Value.CreateDict(x),
            Image           x => Value.CreateImage(x),
            Stream          x => Value.CreateBinary(x),          
            null              => Value.CreateNull(),
            _                 => throw new InvalidOperationException($"Cannot create a Muna value from value '{value}' of type {value.GetType()}"),
        };

        private async Task<RemoteValue> ToRemoteValue(object? value) => value switch { // INCOMPLETE // Image // Json
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
            Json            x when x.IsArray    => new() { data = await Upload(x.ToString().ToStream(), mime: @"application/json"), dtype = Dtype.List },
            Json            x when x.IsObject   => new() { data = await Upload(x.ToString().ToStream(), mime: @"application/json"), dtype = Dtype.Dict },
            Image           x => new() { data = "", dtype = Dtype.Image },
            Stream          x => new() { data = await Upload(x), dtype = Dtype.Binary },
            Enum            x => await ToRemoteValue(x.ToObject()),
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
                Dtype.List      => new Json(new StreamReader(stream).ReadToEnd()),
                Dtype.Dict      => new Json(new StreamReader(stream).ReadToEnd()),
                Dtype.Image     => DeserializeImageValue(stream),
                Dtype.Binary    => stream.Clone(),
                _               => throw new InvalidOperationException($"Failed to deserialize value with type {value.dtype} because it is not supported"),
            };
        }

        private static ValueMap ToValueMap(Dictionary<string, object?> inputs) {
            var map = new ValueMap();
            foreach (var pair in inputs)
                map[pair.Key] = ToValue(pair.Value);
            return map;
        }

        private static Prediction ToPrediction(string tag, C.Prediction prediction) {
            var outputMap = prediction.results;
            return new Prediction {
                id = prediction.id,
                tag = tag,
                created = DateTime.UtcNow,
                results = outputMap != null ? Enumerable.Range(0, outputMap.size)
                    .Select(outputMap.GetKey)
                    .Select(outputMap.GetValue)
                    .Select(value => value.ToObject())
                    .ToArray() : null,
                latency = prediction.latency,
                error = prediction.error,
                logs = prediction.logs,
            };
        }

        private static int ToAcceleration(string? acc) => acc switch {
            @"local_auto"   => 0,
            @"local_cpu"    => 1 << 0,
            @"local_gpu"    => 1 << 1,
            @"local_npu"    => 1 << 2,
            _               => 0,
        };

        private static Image DeserializeImageValue(Stream stream) {
            using var value = Value.CreateFromBinary(stream, @"image/*");
            return (Image)value.ToObject()!;
        }

        private async Task<PredictionResource> Download(PredictionResource resource) {
            var uri = new Uri(resource.url);
            if (uri.IsFile)
                return new() {
                    type = resource.type,
                    url = uri.LocalPath,
                    name = resource.name
                };
            var resourceDir = Path.Combine(client.cachePath, @"cache");
            var path = GetResourcePath(resource, resourceDir);
            if (!File.Exists(path)) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var dataStream = await client.Download(resource.url);
                using var fileStream = File.Create(path);
                dataStream.CopyTo(fileStream); // CHECK // Async usage
            }
            return new() {
                type = resource.type,
                url = path,
                name = resource.name
            };
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

        private Task<string> Upload(
            Stream stream,
            string? mime = @"application/octet-stream"
        ) {
            var data = Convert.ToBase64String(stream.ToArray<byte>());
            var result = $"data:{mime};base64,{data}";
            return Task.FromResult(result);
        }

        internal static string GetResourcePath(
            PredictionResource resource,
            string cacheDir
        ) {
            var uri = new Uri(resource.url);
            var stem = Path.GetFileName(uri.AbsolutePath);
            var path = string.IsNullOrEmpty(resource.name) ?
                Path.Combine(cacheDir, stem) :
                Path.Combine(cacheDir, stem, resource.name);
            return path;
        }
        #endregion


        #region --Types--

        [Preserve, Serializable]
        private class RemotePrediction : Prediction {
            public new RemoteValue[]? results;
        }

        #pragma warning disable 8618
        [Preserve, Serializable]
        private class RemotePredictionEvent {
            [JsonProperty(@"event")]
            public string @event;
            public RemotePrediction data;
        }

        [Preserve, Serializable]
        public sealed class PreloadEntry {
            public string tag;
            public string acceleration;
            public string metadata;
        }
        #pragma warning restore 8618
        #endregion
    }
}