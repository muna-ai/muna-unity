/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using System.Threading.Tasks;
    using NUnit.Framework;
    using Beta.OpenAI;

    internal sealed class OpenAITest {

        private Muna muna;

        [SetUp]
        public void Before() => muna = MunaUnity.Create();

        [Test(Description = @"Should create a chat completion")]
        public async Task CreateChatCompletion() {
            var response = await muna.Beta.OpenAI.Chat.Completions.Create(
                model: "@openai/gpt-oss-20b",
                messages: new[] {
                    new ChatMessage { Role = @"user", Content = @"What is the capital of France?" },
                    new ChatMessage { Role = @"user", Content = @"And how many people live there?" }
                },
                acceleration: Acceleration.Auto
            );
            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Choices);
            Assert.NotNull(response.Choices[0].Message?.Content);
        }

        [Test(Description = @"Should stream a chat completion")]
        public async Task StreamChatCompletion() {
            var chunks = muna.Beta.OpenAI.Chat.Completions.Stream(
                model: "@openai/gpt-oss-20b",
                messages: new[] {
                    new ChatMessage { Role = @"user", Content = @"What is the capital of France?" },
                    new ChatMessage { Role = @"user", Content = @"And how many people live there?" }
                },
                acceleration: Acceleration.Auto
            );
            var count = 0;
            await foreach (var chunk in chunks) {
                Assert.IsInstanceOf<ChatCompletionChunk>(chunk);
                count++;
            }
            Assert.Greater(count, 0);
        }

        [Test(Description = @"Should create an embedding")]
        public async Task CreateEmbedding() {
            var response = await muna.Beta.OpenAI.Embeddings.Create(
                model: "@google/embedding-gemma",
                input: @"Hello world"
            );
            Assert.AreEqual(@"list", response.Object);
            Assert.IsNotEmpty(response.Data);
            Assert.AreEqual(@"embedding", response.Data[0].Object);
            Assert.NotNull(response.Data[0].Floats);
            Assert.IsNotEmpty(response.Data[0].Floats);
        }

        [Test(Description = @"Should create a base64 embedding")]
        public async Task CreateEmbeddingBase64() {
            var response = await muna.Beta.OpenAI.Embeddings.Create(
                model: "@google/embedding-gemma",
                input: @"Hello world",
                encodingFormat: EmbeddingService.EncodingFormat.Base64
            );
            Assert.AreEqual(@"list", response.Object);
            Assert.IsNotEmpty(response.Data);
            Assert.AreEqual(@"embedding", response.Data[0].Object);
            Assert.NotNull(response.Data[0].Base64);
        }

        [Test(Description = @"Should create speech")]
        public async Task CreateSpeech() {
            var response = await muna.Beta.OpenAI.Audio.Speech.Create(
                model: "@kitten-ml/kitten-tts",
                input: @"Hello from Muna",
                voice: @"expr-voice-2-f",
                responseFormat: SpeechService.ResponseFormat.MP3,
                acceleration: Acceleration.Auto
            );
            Assert.NotNull(response);
            Assert.IsFalse(response.IsEmpty);
            Assert.That(response.MediaType, Does.StartWith(@"audio/mp3"));
        }
    }
}