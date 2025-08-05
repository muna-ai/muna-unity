/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Services {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using API;
    using Types;
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
        /// <param name="clientId">Function client identifier. Specify this to override the current client identifier.</param>
        /// <param name="configurationId">Configuration identifier. Specify this to override the current client configuration token.</param>
        public async Task<Prediction> Create (
            string tag,
            Dictionary<string, object?>? inputs = null,
            Acceleration acceleration = default,
            IntPtr device = default,
            string? clientId = default,
            string? configurationId = default
        ) {
            await Configuration.InitializationTask;
            if (inputs == null)
                return await CreateRawPrediction(tag, clientId, configurationId);
            var predictor = await GetPredictor(tag, acceleration, device, clientId, configurationId);
            using var inputMap = ToValueMap(inputs);
            using var prediction = predictor.CreatePrediction(inputMap);
            return ToPrediction(tag, prediction);
        }

        /// <summary>
        /// Stream a prediction.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <param name="device">Prediction device. Do not set this unless you know what you are doing.</param>
        public async IAsyncEnumerable<Prediction> Stream (
            string tag,
            Dictionary<string, object?> inputs,
            Acceleration acceleration = default,
            IntPtr device = default
        ) {
            await Configuration.InitializationTask;
            var predictor = await GetPredictor(tag, acceleration, device);
            using var inputMap = ToValueMap(inputs);
            using var stream = predictor.StreamPrediction(inputMap);
            foreach (var prediction in stream)
                using (prediction)
                    yield return ToPrediction(tag, prediction);
        }

        /// <summary>
        /// Delete a predictor that is loaded in memory.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <returns>Whether the predictor was successfully deleted from memory.</returns>
        public async Task<bool> Delete (string tag) {
            await Configuration.InitializationTask;
            if (!cache.TryGetValue(tag, out var predictor))
                return false;
            predictor.Dispose();
            cache.Remove(tag);
            return true;
        }
        #endregion


        #region --Operations--
        private readonly FunctionClient client;
        private readonly string cachePath;
        private readonly Dictionary<string, C.Predictor> cache = new();

        internal PredictionService (FunctionClient client) {
            this.client = client;
            this.cachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".fxn",
                "cache"
            );
        }

        private Task<Prediction> CreateRawPrediction (
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

        private async Task<C.Predictor> GetPredictor (
            string tag,
            Acceleration acceleration = default,
            IntPtr device = default,
            string? clientId = default,
            string? configurationId = default
        ) {
            if (cache.TryGetValue(tag, out var p))
                return p;
            var prediction = await CreateRawPrediction(tag, clientId, configurationId);
            using var configuration = new Configuration() {
                tag = prediction.tag,
                token = prediction.configuration!,
                acceleration = acceleration,
                device = device
            };
            foreach (var resource in prediction.resources!)
                await configuration.AddResource(
                    resource.type,
                    await DownloadResource(resource)
                );
            var predictor = new C.Predictor(configuration);
            cache.Add(tag, predictor);
            return predictor;
        }

        private async Task<string> DownloadResource (PredictionResource resource) {
            var uri = new Uri(resource.url);
            if (uri.IsFile)
                return uri.LocalPath;
            var path = GetResourcePath(resource, cachePath);
            if (File.Exists(path))
                return path;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using var dataStream = await client.Download(resource.url);
            using var fileStream = File.Create(path);
            dataStream.CopyTo(fileStream); // CHECK // Async usage
            return path;
        }

        internal static string GetResourcePath (PredictionResource resource, string cacheDir) {
            var uri = new Uri(resource.url);
            var stem = Path.GetFileName(uri.AbsolutePath);
            var path = string.IsNullOrEmpty(resource.name) ?
                Path.Combine(cacheDir, stem) :
                Path.Combine(cacheDir, stem, resource.name);
            return path;
        }

        internal static unsafe Value ToValue (object? value) => value switch {
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
            Enum            x => ToValue(SerializeEnum(x)),
            IList           x => Value.CreateList(x),
            IDictionary     x => Value.CreateDict(x),
            Image           x => Value.CreateImage(x),
            Stream          x => Value.CreateBinary(x),          
            null              => Value.CreateNull(),
            _                 => throw new InvalidOperationException($"Cannot create a Function value from value '{value}' of type {value.GetType()}"),
        };

        private static ValueMap ToValueMap (Dictionary<string, object?> inputs) {
            var map = new ValueMap();
            foreach (var pair in inputs)
                map[pair.Key] = ToValue(pair.Value);
            return map;
        }

        private static Prediction ToPrediction (string tag, C.Prediction prediction) {
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

        private static object SerializeEnum (Enum value) {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attribute = fieldInfo?.GetCustomAttributes(typeof(EnumMemberAttribute), false)?.FirstOrDefault() as EnumMemberAttribute;
            return (attribute?.IsValueSetExplicitly ?? false) ? attribute.Value : Convert.ToInt32(value);
        }
        #endregion
    }
}