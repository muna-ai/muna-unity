/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
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
    using Services;
    using PredictorService = global::Muna.Services.PredictorService;
    using EdgePredictionService = global::Muna.Services.PredictionService;

    /// <summary>
    /// Create speech.
    /// </summary>
    public sealed class SpeechService {

        #region --Client API--
        /// <summary>
        /// Audio output format.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ResponseFormat {
            /// <summary>
            /// MP3 audio.
            /// </summary>
            [EnumMember(Value = @"mp3")]
            MP3 = 1,
            /// <summary>
            /// Opus.
            /// </summary>
            [EnumMember(Value = @"opus")]
            Opus = 2,
            /// <summary>
            /// AAC.
            /// </summary>
            [EnumMember(Value = @"aac")]
            AAC = 3,
            /// <summary>
            /// FLAC losseless audio.
            /// </summary>
            [EnumMember(Value = @"flac")]
            FLAC = 4,
            /// <summary>
            /// Waveform audio.
            /// </summary>
            [EnumMember(Value = @"wav")]
            WAV = 5,
            /// <summary>
            /// Linear PCM audio.
            /// </summary>
            [EnumMember(Value = @"pcm")]
            PCM = 6,
        }

        /// <summary>
        /// The format to stream the audio in.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum StreamFormat {
            /// <summary>
            /// Raw audio.
            /// </summary>
            Audio = 1,
            /// <summary>
            /// Server-sent events.
            /// </summary>
            SSE = 2,
        }

        /// <summary>
        /// Generate audio from the input text.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="input"></param>
        /// <param name="voice"></param>
        /// <param name="speed"></param>
        /// <param name="responseFormat"></param>
        /// <param name="streamFormat"></param>
        /// <returns></returns>
        public Task<BinaryData> Create(
            string model,
            string input,
            string voice,
            float speed = 1f,
            ResponseFormat responseFormat = ResponseFormat.MP3,
            StreamFormat streamFormat = StreamFormat.Audio,
            Acceleration acceleration = Acceleration.Auto
        ) => Create(model, input, voice, (object)acceleration, speed, responseFormat, streamFormat);

        /// <summary>
        /// Generate audio from the input text.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="input"></param>
        /// <param name="voice"></param>
        /// <param name="speed"></param>
        /// <param name="responseFormat"></param>
        /// <param name="streamFormat"></param>
        /// <returns></returns>
        public Task<BinaryData> Create(
            string model,
            string input,
            string voice,
            RemoteAcceleration acceleration,
            float speed = 1f,
            ResponseFormat responseFormat = ResponseFormat.MP3,
            StreamFormat streamFormat = StreamFormat.Audio
        ) => Create(model, input, voice, (object)acceleration, speed, responseFormat, streamFormat);
        #endregion


        #region --Operations--
        private readonly PredictorService predictors;
        private readonly EdgePredictionService predictions;
        private readonly RemotePredictionService remotePredictions;
        private readonly Dictionary<string, SpeechDelegate> cache;
        private delegate Task<BinaryData> SpeechDelegate(
            string model,
            string input,
            string voice,
            float speed,
            ResponseFormat responseFormat,
            StreamFormat streamFormat,
            object acceleration
        );

        internal SpeechService(
            PredictorService predictors,
            EdgePredictionService predictions,
            RemotePredictionService remotePredictions
        ) {
            this.predictors = predictors;
            this.predictions = predictions;
            this.remotePredictions = remotePredictions;
            this.cache = new();
        }

        private async Task<BinaryData> Create(
            string model,
            string input,
            string voice,
            object acceleration,
            float speed = 1f,
            ResponseFormat responseFormat = ResponseFormat.MP3,
            StreamFormat streamFormat = StreamFormat.Audio
        ) {
            // Ensure we have a delegate
            if (!cache.ContainsKey(model)) {
                var @delegate = await CreateSpeechDelegate(model);
                cache.Add(model, @delegate);
            }
            // Make prediction
            var handler = cache[model];
            var result = await handler(
                model,
                input,
                voice,
                speed,
                responseFormat,
                streamFormat,
                acceleration
            );
            // Return
            return result;
        }

        private async Task<SpeechDelegate> CreateSpeechDelegate(string tag) {
            // Retrieve predictor
            var predictor = await predictors.Retrieve(tag);
            if (predictor == null)
                throw new ArgumentException($"{tag} cannot be used for OpenAI speech API because the predictor could not be found.");
            // Get required inputs
            var signature = predictor.signature!;
            var requiredInputParams = signature.inputs.Where(parameter => parameter.optional == true).ToArray();
            if (requiredInputParams.Length != 2)
                throw new InvalidOperationException($"${tag} cannot be used with OpenAI speech API because it does not have exactly two required input parameters.");
            // Get the text input parameter
            var inputParam = requiredInputParams.FirstOrDefault(parameter => parameter.type == Dtype.String);
            if (inputParam == null)
                throw new InvalidOperationException($"${tag} cannot be used with OpenAI speech API because it does not have the required speech input parameter.");
            // Get the voice input parameter
            var voiceParam = requiredInputParams.FirstOrDefault(parameter =>
                parameter.type == Dtype.String &&
                parameter.denotation == "audio.voice"
            );
            if (voiceParam == null)
                throw new InvalidOperationException($"${tag} cannot be used with OpenAI speech API because it does not have the required speech voice parameter.");
            // Get the speed input parameter (optional)
            var speedParam = signature.inputs.FirstOrDefault(parameter =>
                new[] { Dtype.Float32, Dtype.Float64 }.Contains((Dtype)parameter.type!) &&
                parameter.denotation == "audio.speed"
            );
            // Get the index of the audio output parameter
            var (audioParamIdx, audioParam) = signature.outputs
                .Select((parameter, idx) => (idx, parameter))
                .Where(pair =>
                    pair.parameter.type == Dtype.Float32 &&
                    pair.parameter.denotation == "audio"
                )
                .FirstOrDefault();
            if (audioParam == null)
                throw new InvalidOperationException($"{tag} cannot be used with OpenAI speech API because it has no outputs with an `audio` denotation.");
            // Create delegate
            SpeechDelegate result = async (
                string model,
                string input,
                string voice,
                float speed,
                ResponseFormat responseFormat,
                StreamFormat streamFormat,
                object acceleration
            ) => {
                // Build prediction input map
                var inputMap = new Dictionary<string, object?> {
                    [inputParam.name] = input,
                    [voiceParam.name] = voice
                };
                if (speedParam != null)
                    inputMap[speedParam.name] = speed;
                // Create prediction
                var prediction = await CreatePrediction(
                    model,
                    inputs: inputMap,
                    acceleration: acceleration
                );
                // Check for error
                if (prediction.error != null)
                    throw new InvalidOperationException(prediction.error);
                // Check returned audio
                var result = prediction.results![audioParamIdx]!;
                if (!(result is Tensor<float> tensor))
                    throw new InvalidOperationException($"{tag} cannot be used with OpenAI speech API because it returned an object of type {result.GetType()} instead of an audio tensor.");
                if (tensor.shape.Length != 1 && tensor.shape.Length != 2) {
                    var shapeStr = "[" + string.Join(", ", tensor.shape) + "]";
                    throw new InvalidOperationException($"{tag} cannot be used with OpenAI speech API because it returned a tensor with an invalid shape: {shapeStr}");
                }
                // Create response
                var channels = tensor.shape.Length == 2 ? tensor.shape[0] : 1; // Assume planar
                var response = new BinaryData( // INCOMPLETE
                    null,
                    mediaType: $"audio/pcm;rate={audioParam.sampleRate};channels=${channels}"
                );
                // Return
                return response;
            };
            // Return
            return result;
        }

        private Task<Prediction> CreatePrediction(
            string tag,
            Dictionary<string, object?> inputs,
            object acceleration
        ) => acceleration switch {
            Acceleration acc        => predictions.Create(tag, inputs, acc),
            RemoteAcceleration acc  => remotePredictions.Create(tag, inputs, acc),
            _                       => throw new InvalidOperationException($"Cannot create {tag} prediction because acceleration is invalid: {acceleration}")
        };
        #endregion
    }
}