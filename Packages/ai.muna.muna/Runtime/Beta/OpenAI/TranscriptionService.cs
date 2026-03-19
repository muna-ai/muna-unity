/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta.OpenAI {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Services;
    using PredictorService = global::Muna.Services.PredictorService;
    using EdgePredictionService = global::Muna.Services.PredictionService;

    /// <summary>
    /// Create transcriptions.
    /// </summary>
    public sealed class TranscriptionService {

        #region --Client API--
        /// <summary>
        /// Transcribe audio into the input language.
        /// </summary>
        /// <param name="model">Audio transcription model tag.</param>
        /// <param name="file">Audio file to transcribe as a `Stream` in `flac`, `mp3`, `m4a`, `ogg`, or `wav` format.</param>
        /// <param name="language">The language of the input audio.</param>
        /// <param name="prompt">Text to guide the model's style or continue a previous audio segment.</param>
        /// <param name="temperature">The sampling temperature, between 0 and 1.</param>
        /// <param name="acceleration">Prediction acceleration. Must be `Acceleration` or `RemoteAcceleration` instance.</param>
        /// <returns>Transcription result.</returns>
        public Task<Transcription> Create(
            string model,
            Stream file,
            string? language = null,
            string? prompt = null,
            float temperature = 0f,
            object? acceleration = null
        ) => Create(
            model,
            file: (object)file,
            language: language,
            prompt: prompt,
            temperature: temperature,
            acceleration: acceleration
        );

        /// <summary>
        /// Transcribe audio into the input language.
        /// </summary>
        /// <param name="model">Audio transcription model tag.</param>
        /// <param name="file">Audio buffer with linear PCM samples to transcribe.</param>
        /// <param name="language">The language of the input audio.</param>
        /// <param name="prompt">Text to guide the model's style or continue a previous audio segment.</param>
        /// <param name="temperature">The sampling temperature, between 0 and 1.</param>
        /// <param name="acceleration">Prediction acceleration. Must be `Acceleration` or `RemoteAcceleration` instance.</param>
        /// <returns>Transcription result.</returns>
        public Task<Transcription> Create(
            string model,
            Audio file,
            string? language = null,
            string? prompt = null,
            float temperature = 0f,
            object? acceleration = null
        ) => Create(
            model,
            file: (object)file,
            language: language,
            prompt: prompt,
            temperature: temperature,
            acceleration: acceleration
        );
        #endregion


        #region --Operations--
        private readonly PredictorService predictors;
        private readonly EdgePredictionService predictions;
        private readonly RemotePredictionService remotePredictions;
        private readonly Dictionary<string, TranscriptionDelegate> cache;
        private delegate Task<Transcription> TranscriptionDelegate(
            string model,
            object file,
            string? language,
            string? prompt,
            float temperature,
            object acceleration
        );

        internal TranscriptionService(
            PredictorService predictors,
            EdgePredictionService predictions,
            RemotePredictionService remotePredictions
        ) {
            this.predictors = predictors;
            this.predictions = predictions;
            this.remotePredictions = remotePredictions;
            this.cache = new();
        }

        private async Task<TranscriptionDelegate> CreateTranscriptionDelegate(string tag) {
            // Retrieve predictor
            var predictor = await predictors.Retrieve(tag);
            if (predictor == null)
                throw new ArgumentException(
                    $"{tag} cannot be used with OpenAI transcription API because " +
                    "the predictor could not be found. Check that your access key " +
                    "is valid and that you have access to the predictor."
                );
            // Get required inputs
            var signature = predictor.signature!;
            var requiredInputParams = signature.inputs.Where(parameter => parameter.optional == false).ToArray();
            if (requiredInputParams.Length != 1)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI transcription API because " +
                    "it has more than one required input parameter."
                );
            // Get the audio input parameter
            var audioParam = requiredInputParams.FirstOrDefault(parameter =>
                parameter.dtype == Dtype.Float32 &&
                parameter.denotation == "audio"
            );
            if (audioParam == null)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI transcription API because " +
                    "it does not have a valid audio input parameter."
                );
            // Get the language parameter (optional)
            var languageParam = signature.inputs.FirstOrDefault(parameter =>
                parameter.dtype == Dtype.String &&
                parameter.denotation == "openai.audio.transcriptions.language"
            );
            // Get the prompt parameter (optional)
            var promptParam = signature.inputs.FirstOrDefault(parameter =>
                parameter.dtype == Dtype.String &&
                parameter.denotation == "openai.audio.transcriptions.prompt"
            );
            // Get the temperature parameter (optional)
            var temperatureParam = signature.inputs.FirstOrDefault(parameter =>
                new[] { Dtype.Float32, Dtype.Float64 }.Contains(parameter.dtype) &&
                parameter.denotation == "openai.chat.completions.temperature"
            );
            // Get the transcription output parameter index
            var (transcriptionParamIdx, transcriptionParam) = signature.outputs
                .Select((parameter, idx) => (idx, parameter))
                .Where(pair => pair.parameter.dtype == Dtype.String)
                .FirstOrDefault();
            if (transcriptionParam == null)
                throw new InvalidOperationException(
                    $"{tag} cannot be used with OpenAI transcription API because " +
                    "it has no output string parameter."
                );
            // Define delegate
            TranscriptionDelegate result = async (
                string model,
                object file,
                string? language,
                string? prompt,
                float temperature,
                object acceleration
            ) => {
                // Read audio samples
                var samples = ReadAudioSamples(file, audioParam.sampleRate!.Value);
                // Build prediction input map
                var inputMap = new Dictionary<string, object?> {
                    [audioParam.name] = samples
                };
                if (language != null && languageParam != null)
                    inputMap[languageParam.name] = language;
                if (prompt != null && promptParam != null)
                    inputMap[promptParam.name] = prompt;
                if (temperatureParam != null)
                    inputMap[temperatureParam.name] = temperature;
                // Create prediction
                var prediction = await CreatePrediction(
                    model,
                    inputs: inputMap,
                    acceleration: acceleration
                );
                // Check for error
                if (prediction.error != null)
                    throw new InvalidOperationException(prediction.error);
                // Check returned transcription
                var rawText = prediction.results![transcriptionParamIdx]!;
                if (!(rawText is string text))
                    throw new InvalidOperationException(
                        $"{tag} returned object of type {rawText.GetType()} instead of a string"
                    );
                // Compute duration
                var elementCount = samples.shape.Aggregate(1, (a, b) => a * b);
                var duration = (float)elementCount / audioParam.sampleRate!.Value;
                // Create result
                var transcription = new Transcription {
                    Text = text,
                    Usage = new Transcription.UsageInfo {
                        Type = "duration",
                        Seconds = duration
                    }
                };
                return transcription;
            };
            // Return
            return result;
        }

        private async Task<Transcription> Create(
            string model,
            object file,
            string? language,
            string? prompt,
            float temperature,
            object? acceleration
        ) {
            // Ensure we have a delegate
            if (!cache.ContainsKey(model)) {
                var @delegate = await CreateTranscriptionDelegate(model);
                cache.Add(model, @delegate);
            }
            // Make prediction
            var handler = cache[model];
            var result = await handler(
                model,
                file,
                language,
                prompt,
                temperature,
                acceleration: acceleration ?? Acceleration.Auto
            );
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

        private static Tensor<float> ReadAudioSamples(object file, int sampleRate) {
            if (file is Audio audio) {
                if (audio.sampleRate != sampleRate)
                    throw new ArgumentException(
                        $"Audio sample rate {audio.sampleRate}Hz does not match " +
                        $"the required sample rate of {sampleRate}Hz."
                    );
                return audio.AsTensor();
            }
            if (file is Stream stream) {
                using var audioValue = C.Value.CreateFromBinary(stream, $"audio/*;rate={sampleRate}");
                var samples = audioValue.ToObject();
                if (samples is Tensor<float> tensor)
                    return tensor;
                throw new InvalidOperationException("Failed to decode audio file into tensor samples");
            }
            throw new ArgumentException($"Unsupported audio file type: {file.GetType()}");
        }
        #endregion
    }
}