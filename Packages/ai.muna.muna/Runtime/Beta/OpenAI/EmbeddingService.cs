/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta.OpenAI {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using Services;

    /// <summary>
    /// Create embeddings.
    /// </summary>
    public sealed class EmbeddingService {

        #region --Client API--
        /// <summary>
        /// Embedding encoding format.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum EncodingFormat {
            /// <summary>
            /// Float array.
            /// </summary>
            [EnumMember(Value = @"float")]
            Float = 1,
            /// <summary>
            /// Base64 string.
            /// </summary>
            [EnumMember(Value = @"base64")]
            Base64 = 2
        }

        /// <summary>
        /// Create an embedding vector representing the input text.
        /// </summary>
        /// <param name="input">Input text to embed. The input must not exceed the max input tokens for the model.</param>
        /// <param name="model">Embedding model predictor tag.</param>
        /// <param name="dimensions">The number of dimensions the resulting output embeddings should have. Only supported by Matryoshka embedding models.</param>
        /// <param name="encodingFormat">The format to return the embeddings in.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <returns>Embeddings.</returns>
        public Task<CreateEmbeddingResponse> Create(
            string model,
            string input,
            int? dimensions = default,
            EncodingFormat encodingFormat = EncodingFormat.Float,
            string? acceleration = default
        ) => Create(
            model,
            new[] { input },
            dimensions: dimensions,
            encodingFormat: encodingFormat,
            acceleration: acceleration
        );

        /// <summary>
        /// Create an embedding vector representing the input text.
        /// </summary>
        /// <param name="input">Input text to embed. The input must not exceed the max input tokens for the model.</param>
        /// <param name="model">Embedding model predictor tag.</param>
        /// <param name="dimensions">The number of dimensions the resulting output embeddings should have. Only supported by Matryoshka embedding models.</param>
        /// <param name="encodingFormat">The format to return the embeddings in.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <returns>Embeddings.</returns>
        public async Task<CreateEmbeddingResponse> Create(
            string model,
            string[] input,
            int? dimensions = null,
            EncodingFormat encodingFormat = EncodingFormat.Float,
            string? acceleration = null
        ) {
            // Ensure we have a delegate
            if (!cache.ContainsKey(model)) {
                var @delegate = await CreateEmbeddingDelegate(model);
                cache.Add(model, @delegate);
            }
            // Make prediction
            var handler = cache[model];
            var result = await handler(
                model,
                input,
                dimensions,
                encodingFormat,
                acceleration: acceleration ?? @"local_auto"
            );
            // Return
            return result;
        }
        #endregion


        #region --Operations--
        private readonly PredictorService predictors;
        private readonly PredictionService predictions;
        private readonly Dictionary<string, EmbeddingDelegate> cache;
        private delegate Task<CreateEmbeddingResponse> EmbeddingDelegate(
            string model,
            string[] input,
            int? dimensions,
            EncodingFormat encodingFormat,
            string acceleration
        );

        internal EmbeddingService(
            PredictorService predictors,
            PredictionService predictions
        ) {
            this.predictors = predictors;
            this.predictions = predictions;
            this.cache = new();
        }

        private async Task<EmbeddingDelegate> CreateEmbeddingDelegate(string tag) {
            // Retrieve predictor
            var predictor = await predictors.Retrieve(tag);
            if (predictor == null)
                throw new ArgumentException(
                    $"{tag} cannot be used with OpenAI embedding API because " +
                    "the predictor could not be found. Check that your access key " +
                    "is valid and that you have access to the predictor."
                );
            // Check that there is only one required input parameter
            var signature = predictor.signature!;
            var requiredInputParams = signature.inputs.Where(parameter => parameter.optional == false).ToArray();
            if (requiredInputParams.Length != 1)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI embedding API because " +
                    "it has more than one required input parameter."
                );
            // Check that the input parameter is `list[str]`
            var inputParam = requiredInputParams.FirstOrDefault(p => p.dtype == Dtype.List);
            if (inputParam == null)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI embedding API because " +
                    "it does not have a valid text embedding input parameter."
                );
            // Get the Matryoshka dim parameter (optional)
            var matryoshkaParam = signature.inputs.FirstOrDefault(parameter =>
                new[] {
                    Dtype.Int8, Dtype.Int16, Dtype.Int32, Dtype.Int64,
                    Dtype.Uint8, Dtype.Uint16, Dtype.Uint32, Dtype.Uint64
                }.Contains(parameter.dtype) &&
                parameter.denotation == "openai.embeddings.dims"
            );
            // Get the embedding output parameter index
            var (embeddingParamIdx, embeddingParam) = signature.outputs
                .Select((parameter, idx) => (idx, parameter))
                .Where(pair =>
                    pair.parameter.dtype == Dtype.Float32 &&
                    pair.parameter.denotation == "embedding"
                )
                .FirstOrDefault();
            if (embeddingParam == null)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI embedding API because " +
                    "it has no outputs with an `embedding` denotation."
                );
            // Get usage output param
            var usageParamIdx = signature.outputs
                .Select((parameter, idx) => (idx, parameter))
                .Where(pair =>
                    pair.parameter.schema != null &&
                    pair.parameter.schema.TryGetValue("title", out var title) &&
                    title?.ToString() == "Usage"
                )
                .Select(pair => (int?)pair.idx)
                .FirstOrDefault();
            // Define delegate
            EmbeddingDelegate result = async (
                string model,
                string[] input,
                int? dimensions,
                EncodingFormat encodingFormat,
                string acceleration
            ) => {
                // Build prediction input map
                var inputMap = new Dictionary<string, object?> {
                    [inputParam.name] = input
                };
                if (dimensions != null && matryoshkaParam != null)
                    inputMap[matryoshkaParam.name] = dimensions.Value;
                // Create prediction
                var prediction = await predictions.Create(
                    model,
                    inputs: inputMap,
                    acceleration: acceleration
                );
                // Check for error
                if (prediction.error != null)
                    throw new InvalidOperationException(prediction.error);
                // Check embedding return type
                var rawEmbeddingMatrix = prediction.results![embeddingParamIdx]!;
                if (!(rawEmbeddingMatrix is Tensor<float> embeddingMatrix))
                    throw new InvalidOperationException(
                        $"{tag} returned object of type {rawEmbeddingMatrix.GetType()} instead of an embedding matrix"
                    );
                if (embeddingMatrix.shape.Length != 2) {
                    var shapeStr = "(" + string.Join(",", embeddingMatrix.shape) + ")";
                    throw new InvalidOperationException(
                        $"{tag} returned embedding matrix with invalid shape: {shapeStr}"
                    );
                }
                // Create embedding response
                var embeddings = Enumerable
                    .Range(0, embeddingMatrix.shape[0])
                    .Select(idx => ParseEmbedding(embeddingMatrix, idx, encodingFormat))
                    .ToArray();
                var usage = usageParamIdx != null ?
                    (prediction.results![usageParamIdx.Value]! as JObject)!.ToObject<CreateEmbeddingResponse.UsageInfo>() :
                    new CreateEmbeddingResponse.UsageInfo { PromptTokens = 0, TotalTokens = 0 };
                var response = new CreateEmbeddingResponse {
                    Object = "list",
                    Model = model,
                    Data = embeddings,
                    Usage = usage
                };
                // Return
                return response;
            };
            // Return
            return result;
        }

        private unsafe Embedding ParseEmbedding(
            Tensor<float> matrix,
            int index,
            EncodingFormat format
        ) {
            fixed (float* data = matrix) {
                var baseAddress = data + index * matrix.shape[1];
                var floatSpan = new ReadOnlySpan<float>(baseAddress, matrix.shape[1]);
                var byteSpan = new ReadOnlySpan<byte>(baseAddress, matrix.shape[1] * sizeof(float));
                var embeddingVector = format == EncodingFormat.Float ? floatSpan.ToArray() : null;
                var base64Rep = format == EncodingFormat.Base64 ? Convert.ToBase64String(byteSpan) : null;
                var embedding = new Embedding {
                    Object = @"embedding",
                    Floats = embeddingVector,
                    Index = index,
                    Base64 = base64Rep
                };
                return embedding;
            }
        }
        #endregion
    }
}