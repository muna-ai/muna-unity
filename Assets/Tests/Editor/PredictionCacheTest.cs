/*
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Tests {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Newtonsoft.Json;
    using global::Muna.API;
    using global::Muna.Services;
    using Configuration = global::Muna.C.Configuration;

    internal sealed class PredictionCacheTest {

        [Test]
        public async Task FirstRetrieveFetchesAndPersists() {
            var client = new FakeClient {
                handler = _ => CreatePrediction(@"first", iat: 100, exp: 1_100)
            };
            var cache = new PredictionCache(client, () => 150);
            var prediction = await cache.Retrieve(@"@test/model");
            Assert.AreEqual(@"first", prediction.id);
            Assert.AreEqual(1, client.requests.Count);
            Assert.AreEqual(1, client.store.Count);
        }

        [Test]
        public async Task FreshTokenIsServedFromCache() {
            var client = new FakeClient {
                handler = _ => CreatePrediction(@"first", iat: 100, exp: 1_100)
            };
            var cache = new PredictionCache(client, () => 599);
            await cache.Retrieve(@"@test/model");
            var prediction = await cache.Retrieve(@"@test/model");
            Assert.AreEqual(@"first", prediction.id);
            Assert.AreEqual(1, client.requests.Count);
        }

        [Test]
        public async Task HalfLifeRefreshIsPinnedToCachedPrediction() {
            var now = 150L;
            var client = new FakeClient {
                handler = _ => CreatePrediction(@"first", iat: 100, exp: 1_100)
            };
            var cache = new PredictionCache(client, () => now);
            await cache.Retrieve(@"@test/model");

            now = 600;
            client.handler = _ => CreatePrediction(@"second", iat: 600, exp: 1_600);
            var prediction = await cache.Retrieve(@"@test/model");

            Assert.AreEqual(@"second", prediction.id);
            Assert.AreEqual(2, client.requests.Count);
            Assert.AreEqual(@"first", client.requests.Last()[@"predictionId"]);
        }

        [Test]
        public async Task RefreshFailureFallsBackToStalePrediction() {
            var now = 150L;
            var client = new FakeClient {
                handler = _ => CreatePrediction(@"first", iat: 100, exp: 1_100)
            };
            var cache = new PredictionCache(client, () => now);
            await cache.Retrieve(@"@test/model");

            now = 1_200;
            client.handler = null;
            var prediction = await cache.Retrieve(@"@test/model");

            Assert.AreEqual(@"first", prediction.id);
        }

        [Test]
        public async Task InvalidateEvictsAndPinsNextFetch() {
            var client = new FakeClient {
                handler = _ => CreatePrediction(@"first", iat: 100, exp: 1_100)
            };
            var cache = new PredictionCache(client, () => 150);
            await cache.Retrieve(@"@test/model");

            await cache.Invalidate(@"@test/model");

            Assert.AreEqual(0, client.store.Count);
            client.handler = _ => CreatePrediction(@"second", iat: 200, exp: 1_200);
            var prediction = await cache.Retrieve(@"@test/model");
            Assert.AreEqual(@"second", prediction.id);
            Assert.AreEqual(@"first", client.requests.Last()[@"predictionId"]);
        }

        [Test]
        public async Task InvalidatedPredictionIsNeverReserved() {
            var client = new FakeClient {
                handler = _ => CreatePrediction(@"first", iat: 100, exp: 1_100)
            };
            var cache = new PredictionCache(client, () => 150);
            await cache.Retrieve(@"@test/model");

            await cache.Invalidate(@"@test/model");
            client.handler = null;

            Assert.ThrowsAsync<InvalidOperationException>(
                () => cache.Retrieve(@"@test/model")
            );
        }

        [Test]
        public async Task PinnedPredictionIsUsedOnFirstFetch() {
            await Configuration.InitializationTask;
            var client = new FakeClient {
                handler = _ => CreatePrediction(@"fresh", iat: 100, exp: 1_100)
            };
            var cache = new PredictionCache(client, () => 150);
            cache.Pin(@"@test/model", Configuration.ClientId, @"embedded");

            await cache.Retrieve(@"@test/model");

            Assert.AreEqual(@"embedded", client.requests.Last()[@"predictionId"]);
        }

        [Test]
        public async Task RawPredictionBypassesCache() {
            var client = new FakeClient {
                handler = _ => CreatePrediction(@"raw", iat: 100, exp: 1_100)
            };
            var service = new PredictionService(client);

            await service.Create(
                @"@test/model",
                inputs: null,
                clientId: @"client",
                configurationId: @"configuration"
            );

            Assert.AreEqual(1, client.requests.Count);
            Assert.AreEqual(0, client.store.Count);
        }

        [Test]
        public void LegacyTokenDoesNotRefresh() {
            var token = CreateToken(iat: 100);

            Assert.False(PredictionCache.ShouldRefreshToken(token, now: 10_000));
        }

        [Test]
        public void TokenRefreshesAtHalfLife() {
            var token = CreateToken(iat: 100, exp: 1_100);

            Assert.False(PredictionCache.ShouldRefreshToken(token, now: 599));
            Assert.True(PredictionCache.ShouldRefreshToken(token, now: 600));
        }

        [Test]
        public void MalformedTokenRefreshes() {
            Assert.True(PredictionCache.ShouldRefreshToken(@"not-a-token", now: 0));
        }

        [Test]
        public void CacheKeyIncludesClientAndConfiguration() {
            var key = PredictionCache.GetCacheKey(
                @"tag",
                @"client-a",
                @"configuration-a"
            );

            Assert.AreNotEqual(
                key,
                PredictionCache.GetCacheKey(@"tag", @"client-b", @"configuration-a")
            );
            Assert.AreNotEqual(
                key,
                PredictionCache.GetCacheKey(@"tag", @"client-a", @"configuration-b")
            );
        }

        private static Prediction CreatePrediction(
            string id,
            long iat,
            long? exp = null
        ) => new() {
            id = id,
            tag = @"@test/model",
            created = DateTime.UtcNow,
            configuration = CreateToken(iat, exp),
            resources = Array.Empty<PredictionResource>(),
        };

        private static string CreateToken(long iat, long? exp = null) {
            var header = Encode(new { alg = @"EdDSA", typ = @"JWT" });
            var payload = Encode(new { iat, exp });
            return $"{header}.{payload}.signature";
        }

        private static string Encode(object value) {
            var json = JsonConvert.SerializeObject(
                value,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
            );
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private sealed class FakeClient : MunaClient {

            public readonly Dictionary<string, string> store = new();
            public readonly List<Dictionary<string, object?>> requests = new();
            public Func<Dictionary<string, object?>?, Prediction>? handler;

            public FakeClient() : base(@"https://api.muna.ai", null) { }

            public override Task<T?> Request<T>(
                string method,
                string path,
                Dictionary<string, object?>? payload = null
            ) where T : class {
                requests.Add(payload ?? new());
                if (handler == null)
                    throw new InvalidOperationException(@"offline");
                return Task.FromResult(handler(payload) as T);
            }

            public override string? GetCacheEntry(string key) =>
                store.TryGetValue(key, out var value) ? value : null;

            public override void SetCacheEntry(string key, string? value) {
                if (value != null)
                    store[key] = value;
                else
                    store.Remove(key);
            }

            public override async IAsyncEnumerable<T> Stream<T>(
                string method,
                string path,
                Dictionary<string, object?>? payload = null
            ) where T : class {
                await Task.CompletedTask;
                yield break;
            }

            public override Task<System.IO.Stream> Download(string url) =>
                throw new NotSupportedException();

            public override Task Upload(
                System.IO.Stream stream,
                string url,
                string? mime = null
            ) => throw new NotSupportedException();
        }
    }
}
