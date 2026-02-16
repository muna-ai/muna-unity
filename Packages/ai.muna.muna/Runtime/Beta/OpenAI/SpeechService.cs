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
        /// <param name="model">Speech generation predictor tag.</param>
        /// <param name="input">The text to generate audio for.</param>
        /// <param name="voice">The voice to use when generating the audio.</param>
        /// <param name="speed">The speed of the generated audio. Defaults to 1.0.</param>
        /// <param name="responseFormat">Audio output format.</param>
        /// <param name="streamFormat">The format to stream the audio in.</param>
        /// <param name="acceleration">Prediction acceleration. Must be `Acceleration` or `RemoteAcceleration` instance.</param>
        /// <returns>Generated audio.</returns>
        public async Task<BinaryData> Create(
            string model,
            string input,
            string voice,
            float speed = 1f,
            ResponseFormat responseFormat = ResponseFormat.MP3,
            StreamFormat streamFormat = StreamFormat.Audio,
            object? acceleration = null
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
                acceleration: acceleration ?? Acceleration.Auto
            );
            // Return
            return result;
        }
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

        private async Task<SpeechDelegate> CreateSpeechDelegate(string tag) {
            // Retrieve predictor
            var predictor = await predictors.Retrieve(tag);
            if (predictor == null)
                throw new ArgumentException(
                    $"{tag} cannot be used with OpenAI speech API because " +
                    "the predictor could not be found. Check that your access key " +
                    "is valid and that you have access to the predictor."
                );
            // Get required inputs
            var signature = predictor.signature!;
            var requiredInputParams = signature.inputs.Where(parameter => parameter.optional == false).ToArray();
            if (requiredInputParams.Length != 2)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI speech API because " +
                    "it does not have exactly two required input parameters."
                );
            // Get the text input parameter
            var inputParam = requiredInputParams.FirstOrDefault(parameter => parameter.dtype == Dtype.String);
            if (inputParam == null)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI speech API because " +
                    "it does not have the required speech input parameter."
                );
            // Get the voice input parameter
            var voiceParam = requiredInputParams.FirstOrDefault(parameter =>
                parameter.dtype == Dtype.String &&
                parameter.denotation == "openai.audio.speech.voice"
            );
            if (voiceParam == null)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI speech API because " +
                    "it does not have the required speech voice parameter."
                );
            // Get the speed input parameter (optional)
            var speedParam = signature.inputs.FirstOrDefault(parameter =>
                new[] { Dtype.Float32, Dtype.Float64 }.Contains(parameter.dtype) &&
                parameter.denotation == "openai.audio.speech.speed"
            );
            // Get the audio output parameter index
            var (audioParamIdx, audioParam) = signature.outputs
                .Select((parameter, idx) => (idx, parameter))
                .Where(pair =>
                    pair.parameter.dtype == Dtype.Float32 &&
                    pair.parameter.denotation == "audio"
                )
                .FirstOrDefault();
            if (audioParam == null)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI speech API because " +
                    "it has no outputs with an `audio` denotation."
                );
            // Define delegate
            SpeechDelegate result = async (
                string model,
                string input,
                string voice,
                float speed,
                ResponseFormat responseFormat,
                StreamFormat streamFormat,
                object acceleration
            ) => {
                // Check stream format
                if (streamFormat != StreamFormat.Audio)
                    throw new ArgumentException(
                        $"Cannot create speech with stream format `{streamFormat}` " +
                        "because only `Audio` is currently supported."
                    );
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
                var rawAudio = prediction.results![audioParamIdx]!;
                if (!(rawAudio is Tensor<float> audio))
                    throw new InvalidOperationException(
                        $"{tag} returned object of type {rawAudio.GetType()} instead of an audio tensor"
                    );
                if (audio.shape.Length != 1 && audio.shape.Length != 2)
                    throw new InvalidOperationException(
                        $"{tag} returned audio tensor with invalid shape: ({string.Join(",", audio.shape)})"
                    );
                // Create response
                var (content, contentType) = CreateResponseData(
                    audio,
                    sampleRate: audioParam.sampleRate!.Value,
                    responseFormat: responseFormat
                );
                var response = new BinaryData(content, contentType);
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
            _ => throw new InvalidOperationException($"Cannot create {tag} prediction because acceleration is invalid: {acceleration}")
        };

        private static unsafe (byte[] content, string contentType) CreateResponseData(
            Tensor<float> audio,
            int sampleRate,
            ResponseFormat responseFormat
        ) {
            var channels = audio.shape.Length == 2 ? audio.shape[1] : 1; // Assume interleaved
            if (responseFormat == ResponseFormat.PCM) {
                var contentType = string.Join(";",
                    "audio/pcm",
                    $"rate={sampleRate}",
                    $"channels={channels}",
                    "encoding=float",
                    "bits=32"
                );
                var elementCount = audio.shape.Aggregate(1, (a, b) => a * b);
                var data = new byte[elementCount * sizeof(float)];
                fixed (float* src = audio)
                    fixed (byte* dst = data)
                        Buffer.MemoryCopy(src, dst, data.Length, data.Length);
                return (data, contentType);
            }
            using var audioValue = C.Value.CreateArray(audio, C.Value.Flags.CopyData);
            var mime = $"audio/{EdgePredictionService.SerializeEnum(responseFormat)};rate={sampleRate}";
            var serializedData = audioValue.Serialize(mime);
            return (serializedData, mime);
        }
        #endregion
    }
}