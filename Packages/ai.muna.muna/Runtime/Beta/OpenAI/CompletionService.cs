/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta.OpenAI {

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Services;
    using PredictorService = global::Muna.Services.PredictorService;
    using EdgePredictionService = global::Muna.Services.PredictionService;

    /// <summary>
    /// Create chat conversations.
    /// </summary>
    public sealed class ChatCompletionService {

        #region --Client API--
        /// <summary>
        /// Create a chat completion.
        /// </summary>
        /// <param name="model">Chat model predictor tag.</param>
        /// <param name="messages">Messages comprising the conversation so far.</param>
        /// <param name="maxTokens">Maximum output tokens.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        public async Task<ChatCompletion> Create(
            string model,
            ChatMessage[] messages,
            int? maxTokens = null,
            object? acceleration = null
        ) {
            var prediction = await CreatePrediction(
                model,
                new() {
                    [@"messages"] = messages,
                    [@"max_tokens"] = maxTokens
                },
                acceleration: acceleration ?? Acceleration.Auto
            );
            if (prediction.error != null)
                throw new InvalidOperationException(prediction.error);
            var completion = (prediction.results![0] as JObject)!.ToObject<ChatCompletion>()!;
            return completion;
        }

        /// <summary>
        /// Stream a chat completion.
        /// </summary>
        /// <param name="model">Chat model predictor tag.</param>
        /// <param name="messages">Messages comprising the conversation so far.</param>
        /// <param name="maxTokens">Maximum output tokens.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        public async IAsyncEnumerable<ChatCompletionChunk> Stream(
            string model,
            ChatMessage[] messages,
            int? maxTokens = null,
            object? acceleration = null
        ) {
            var stream = StreamPrediction(
                model,
                new() {
                    [@"messages"] = messages,
                    [@"max_tokens"] = maxTokens
                },
                acceleration: acceleration ?? Acceleration.Auto
            );
            await foreach (var prediction in stream) {
                if (prediction.error != null)
                    throw new InvalidOperationException(prediction.error);
                var chunk = (prediction.results![0] as JObject)!.ToObject<ChatCompletionChunk>()!;
                yield return chunk;
            }
        }
        #endregion


        #region --Operations--
        private readonly PredictorService predictors;
        private readonly EdgePredictionService predictions;
        private readonly RemotePredictionService remotePredictions;

        internal ChatCompletionService(
            PredictorService predictors,
            EdgePredictionService predictions,
            RemotePredictionService remotePredictions
        ) {
            this.predictors = predictors;
            this.predictions = predictions;
            this.remotePredictions = remotePredictions;
        }

        private Task<Prediction> CreatePrediction(
            string tag,
            Dictionary<string, object?> inputs,
            object acceleration
        ) => acceleration switch {
            Acceleration acc => predictions.Create(tag, inputs, acc),
            RemoteAcceleration acc => remotePredictions.Create(tag, inputs, acc),
            _ => throw new InvalidOperationException($"Cannot create {tag} prediction because acceleration is invalid: {acceleration}")
        };

        private IAsyncEnumerable<Prediction> StreamPrediction(
            string tag,
            Dictionary<string, object?> inputs,
            object acceleration
        ) => acceleration switch {
            Acceleration acc => predictions.Stream(tag, inputs, acc),
            _ => throw new InvalidOperationException($"Cannot stream {tag} prediction because acceleration is invalid: {acceleration}")
        };
        #endregion
    }
}