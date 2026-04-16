/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta.OpenAI {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Services;

    /// <summary>
    /// Create chat completions.
    /// </summary>
    public sealed class ChatCompletionService {

        #region --Client API--
        /// <summary>
        /// Create a chat completion.
        /// </summary>
        /// <param name="model">Chat model predictor tag.</param>
        /// <param name="messages">Messages comprising the conversation so far.</param>
        /// <param name="responseFormat">Response format.</param>
        /// <param name="reasoningEffort">Reasoning effort for reasoning models.</param>
        /// <param name="maxCompletionTokens">Maximum completion tokens.</param>
        /// <param name="temperature">Sampling temperature to use.</param>
        /// <param name="topP">Nucleus sampling coefficient.</param>
        /// <param name="frequencyPenalty">Token frequency penalty.</param>
        /// <param name="presencePenalty">Token presence penalty.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <returns>Chat completion.</returns>
        public async Task<ChatCompletion> Create(
            string model,
            ChatMessage[] messages,
            Dictionary<string, object?>? responseFormat = null,
            string? reasoningEffort = null,
            int? maxCompletionTokens = null,
            float? temperature = null,
            float? topP = null,
            float? frequencyPenalty = null,
            float? presencePenalty = null,
            string? acceleration = null
        ) {
            // Ensure we have a delegate
            if (!cache.ContainsKey(model))
                cache[model] = await CreateCompletionDelegate(model);
            // Make prediction
            var handler = cache[model];
            var result = await handler(
                model,
                messages,
                stream: false,
                responseFormat,
                reasoningEffort,
                maxCompletionTokens,
                temperature,
                topP,
                frequencyPenalty,
                presencePenalty,
                acceleration: acceleration
            );
            // Return
            return (ChatCompletion)result;
        }

        /// <summary>
        /// Stream a chat completion.
        /// </summary>
        /// <param name="model">Chat model predictor tag.</param>
        /// <param name="messages">Messages comprising the conversation so far.</param>
        /// <param name="responseFormat">Response format.</param>
        /// <param name="reasoningEffort">Reasoning effort for reasoning models.</param>
        /// <param name="maxCompletionTokens">Maximum completion tokens.</param>
        /// <param name="temperature">Sampling temperature to use.</param>
        /// <param name="topP">Nucleus sampling coefficient.</param>
        /// <param name="frequencyPenalty">Token frequency penalty.</param>
        /// <param name="presencePenalty">Token presence penalty.</param>
        /// <param name="acceleration">Prediction acceleration.</param>
        /// <returns>Chat completion chunks.</returns>
        public async IAsyncEnumerable<ChatCompletionChunk> Stream(
            string model,
            ChatMessage[] messages,
            Dictionary<string, object?>? responseFormat = default,
            string? reasoningEffort = default,
            int? maxCompletionTokens = default,
            float? temperature = default,
            float? topP = default,
            float? frequencyPenalty = default,
            float? presencePenalty = default,
            string? acceleration = default
        ) {
            // Ensure we have a delegate
            if (!cache.ContainsKey(model))
                cache[model] = await CreateCompletionDelegate(model);
            // Make prediction
            var handler = cache[model];
            var result = await handler(
                model,
                messages,
                stream: true,
                responseFormat,
                reasoningEffort,
                maxCompletionTokens,
                temperature,
                topP,
                frequencyPenalty,
                presencePenalty,
                acceleration: acceleration ?? @"local_auto"
            );
            // Return
            var stream = (IAsyncEnumerable<ChatCompletionChunk>)result;
            await foreach (var chunk in stream)
                yield return chunk;
        }
        #endregion


        #region --Operations--
        private readonly PredictorService predictors;
        private readonly PredictionService predictions;
        private readonly Dictionary<string, CompletionDelegate> cache;
        private delegate Task<object> CompletionDelegate(
            string model,
            ChatMessage[] messages,
            bool stream,
            Dictionary<string, object?>? responseFormat,
            string? reasoningEffort,
            int? maxCompletionTokens,
            float? temperature,
            float? topP,
            float? frequencyPenalty,
            float? presencePenalty,
            string? acceleration
        );

        internal ChatCompletionService(
            PredictorService predictors,
            PredictionService predictions
        ) {
            this.predictors = predictors;
            this.predictions = predictions;
            this.cache = new();
        }

        private async Task<CompletionDelegate> CreateCompletionDelegate(string tag) {
            // Retrieve predictor
            var predictor = await predictors.Retrieve(tag);
            if (predictor == null)
                throw new ArgumentException(
                    $"{tag} cannot be used with OpenAI chat completions API because " +
                    "the predictor could not be found. Check that your access key " +
                    "is valid and that you have access to the predictor."
                );
            // Check that there is only one required input parameter
            var signature = predictor.signature!;
            var requiredInputs = signature.inputs.Where(p => p.optional == false).ToArray();
            if (requiredInputs.Length != 1)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI chat completions API because " +
                    "it has more than one required input parameter."
                );
            // Check that the input parameter is `list[Message]`
            var inputParam = requiredInputs.FirstOrDefault(p => p.dtype == Dtype.List);
            if (inputParam == null)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI chat completions API because " +
                    "it does not have a valid chat messages input parameter."
                );
            // Get optional inputs
            var responseFormatParam = signature.inputs.FirstOrDefault(p =>
                p.dtype == Dtype.Dict &&
                p.denotation == "openai.chat.completions.response_format"
            );
            var reasoningEffortParam = signature.inputs.FirstOrDefault(p =>
                p.dtype == Dtype.String &&
                p.denotation == "openai.chat.completions.reasoning_effort"
            );
            var maxOutputTokensParam = signature.inputs.FirstOrDefault(p =>
                new[] {
                    Dtype.Int8, Dtype.Int16, Dtype.Int32, Dtype.Int64,
                    Dtype.Uint8, Dtype.Uint16, Dtype.Uint32, Dtype.Uint64
                }.Contains(p.dtype) &&
                p.denotation == "openai.chat.completions.max_output_tokens"
            );
            var temperatureParam = signature.inputs.FirstOrDefault(p =>
                new[] { Dtype.Float32, Dtype.Float64 }.Contains(p.dtype) &&
                p.denotation == "openai.chat.completions.temperature"
            );
            var topPParam = signature.inputs.FirstOrDefault(p =>
                new[] { Dtype.Float32, Dtype.Float64 }.Contains(p.dtype) &&
                p.denotation == "openai.chat.completions.top_p"
            );
            var frequencyPenaltyParam = signature.inputs.FirstOrDefault(p =>
                new[] { Dtype.Float32, Dtype.Float64 }.Contains(p.dtype) &&
                p.denotation == "openai.chat.completions.frequency_penalty"
            );
            var presencePenaltyParam = signature.inputs.FirstOrDefault(p =>
                new[] { Dtype.Float32, Dtype.Float64 }.Contains(p.dtype) &&
                p.denotation == "openai.chat.completions.presence_penalty"
            );
            // Get chat completion output param
            var completionParamIdx = signature.outputs
                .Select((parameter, idx) => (idx, parameter))
                .Where(pair =>
                    pair.parameter.dtype == Dtype.Dict &&
                    pair.parameter.schema != null &&
                    pair.parameter.schema.TryGetValue("title", out var title) &&
                    (title?.ToString() == "ChatCompletion" || title?.ToString() == "ChatCompletionChunk")
                )
                .Select(pair => (int?)pair.idx)
                .FirstOrDefault();
            if (completionParamIdx == null)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI chat completions API because " +
                    "it does not have a valid chat completion output parameter."
                );
            // Define delegate
            CompletionDelegate result = async (
                string model,
                ChatMessage[] messages,
                bool stream,
                Dictionary<string, object?>? responseFormat,
                string? reasoningEffort,
                int? maxCompletionTokens,
                float? temperature,
                float? topP,
                float? frequencyPenalty,
                float? presencePenalty,
                string? acceleration
            ) => {
                // Build prediction input map
                var inputMap = new Dictionary<string, object?> { [inputParam.name] = messages };
                if (responseFormatParam != null && responseFormat != null)
                    inputMap[responseFormatParam.name] = responseFormat;
                if (reasoningEffortParam != null && reasoningEffort != null)
                    inputMap[reasoningEffortParam.name] = reasoningEffort;
                if (maxOutputTokensParam != null && maxCompletionTokens != null)
                    inputMap[maxOutputTokensParam.name] = maxCompletionTokens.Value;
                if (temperatureParam != null && temperature != null)
                    inputMap[temperatureParam.name] = temperature.Value;
                if (topPParam != null && topP != null)
                    inputMap[topPParam.name] = topP.Value;
                if (frequencyPenaltyParam != null && frequencyPenalty != null)
                    inputMap[frequencyPenaltyParam.name] = frequencyPenalty.Value;
                if (presencePenaltyParam != null && presencePenalty != null)
                    inputMap[presencePenaltyParam.name] = presencePenalty.Value;
                // Stream predictions
                var predictionStream = predictions.Stream(model, inputMap, acceleration);
                if (stream)
                    return (object)GatherCompletionChunks(predictionStream, completionParamIdx.Value);
                else
                    return (object)await GatherChatCompletion(predictionStream, completionParamIdx.Value);
            };
            // Return
            return result;
        }

        private static async Task<ChatCompletion> GatherChatCompletion(
            IAsyncEnumerable<Prediction> predictions,
            int completionParamIdx
        ) {
            var outputs = new List<JObject>();
            await foreach (var prediction in predictions) {
                if (prediction.error != null)
                    throw new InvalidOperationException(prediction.error);
                outputs.Add((prediction.results![completionParamIdx] as JObject)!);
            }
            return ParseChatCompletion(outputs);
        }

        private static async IAsyncEnumerable<ChatCompletionChunk> GatherCompletionChunks(
            IAsyncEnumerable<Prediction> predictions,
            int completionParamIdx
        ) {
            await foreach (var prediction in predictions) {
                if (prediction.error != null)
                    throw new InvalidOperationException(prediction.error);
                var output = (prediction.results![completionParamIdx] as JObject)!;
                yield return ParseChatCompletionChunk(output);
            }
        }

        private static ChatCompletion ParseChatCompletion(List<JObject> outputs) {
            if (outputs.Count == 0)
                throw new InvalidOperationException(
                    "Failed to parse chat completion because model did not return any outputs"
                );
            if (outputs.All(o => o["object"]?.ToString() == "chat.completion")) {
                var completions = outputs.Select(o => o.ToObject<ChatCompletion>()!).ToList();
                return completions.Last();
            }
            if (outputs.All(o => o["object"]?.ToString() == "chat.completion.chunk")) {
                var chunks = outputs.Select(o => o.ToObject<ChatCompletionChunk>()!).ToList();
                return MergeChunks(chunks);
            }
            throw new InvalidOperationException(
                "Failed to parse chat completion from model outputs"
            );
        }

        private static ChatCompletionChunk ParseChatCompletionChunk(JObject output) {
            // Try as ChatCompletionChunk
            if (output["object"]?.ToString() == "chat.completion.chunk")
                return output.ToObject<ChatCompletionChunk>()!;
            // Try as ChatCompletion and convert to chunk
            if (output["object"]?.ToString() == "chat.completion") {
                var completion = output.ToObject<ChatCompletion>()!;
                return new ChatCompletionChunk {
                    Object = "chat.completion.chunk",
                    Id = completion.Id,
                    Created = completion.Created,
                    Model = completion.Model,
                    Choices = completion.Choices.Select(choice => new ChatCompletionChunk.Choice {
                        Index = choice.Index,
                        Delta = new ChatCompletionChunk.Choice.MessageDelta {
                            Role = choice.Message.Role,
                            Content = choice.Message.Content
                        },
                        FinishReason = choice.FinishReason
                    }).ToArray(),
                    Usage = completion.Usage
                };
            }
            throw new InvalidOperationException(
                "Failed to parse streaming chat completion chunk from model output"
            );
        }

        private static ChatCompletion MergeChunks(List<ChatCompletionChunk> chunks) {
            var choicesMap = new Dictionary<int, List<ChatCompletionChunk.Choice>>();
            foreach (var chunk in chunks)
                foreach (var choice in chunk.Choices) {
                    if (!choicesMap.ContainsKey(choice.Index))
                        choicesMap[choice.Index] = new();
                    choicesMap[choice.Index].Add(choice);
                }
            var choices = choicesMap
                .Select(pair => CreateCompletionChoice(pair.Key, pair.Value))
                .ToArray();
            var usages = chunks
                .Where(c => c.Usage != null)
                .Select(c => c.Usage!.Value)
                .ToList();
            var usage = new ChatCompletion.UsageInfo {
                PromptTokens = usages.Sum(u => u.PromptTokens),
                CompletionTokens = usages.Sum(u => u.CompletionTokens),
                TotalTokens = usages.Sum(u => u.TotalTokens)
            };
            return new ChatCompletion {
                Object = "chat.completion",
                Id = chunks[0].Id,
                Created = chunks[0].Created,
                Model = chunks[0].Model,
                Choices = choices,
                Usage = usage
            };
        }

        private static ChatCompletion.Choice CreateCompletionChoice(
            int index,
            List<ChatCompletionChunk.Choice> choices
        ) {
            var role = choices
                .Select(c => c.Delta?.Role)
                .FirstOrDefault(r => r != null) ?? @"assistant";
            var content = string.Join("",
                choices
                    .Where(c => c.Delta?.Content != null)
                    .Select(c => c.Delta!.Content)
            );
            var finishReason = choices
                .Select(c => c.FinishReason)
                .FirstOrDefault(r => r != null);
            return new ChatCompletion.Choice {
                Index = index,
                Message = new ChatMessage {
                    Role = role,
                    Content = content
                },
                FinishReason = finishReason
            };
        }
        #endregion
    }
}