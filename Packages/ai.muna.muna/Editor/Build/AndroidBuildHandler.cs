/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Android;
    using UnityEditor.Build.Reporting;
    using API;
    using Services;
    using Types;
    using FunctionSettings = Internal.FunctionSettings;

    internal sealed class AndroidBuildHandler : BuildHandler, IPostGenerateGradleAndroidProject {

        private static List<CachedPrediction> cache;
        private static Dictionary<AndroidArchitecture, string> ArchToClientId = new () {
            [AndroidArchitecture.ARMv7]     = @"android-armeabi-v7a",
            [AndroidArchitecture.ARM64]     = @"android-arm64-v8a",
            [AndroidArchitecture.X86_64]    = @"android-x86_64",
        };

        protected override BuildTarget[] targets => new [] { BuildTarget.Android };

        protected override FunctionSettings CreateSettings (BuildReport report) {
            var projectSettings = FunctionProjectSettings.instance;
            var settings = FunctionSettings.Create(projectSettings.accessKey);
            var embeds = GetEmbeds();
            var clientIds = ArchToClientId
                .Where(pair => PlayerSettings.Android.targetArchitectures.HasFlag(pair.Key))
                .Select(pair => pair.Value)
                .ToArray();
            var cache = embeds
                .SelectMany(embed => {
                    var client = new DotNetClient(embed.url, embed.accessKey);
                    var fxn = new Function(client);
                    var predictions = clientIds.SelectMany(clientId => embed.tags.Select(tag => {
                        try {
                            var prediction = Task.Run(() => fxn.Predictions.Create(
                                tag,
                                clientId: clientId,
                                configurationId: @""
                            )).Result;
                            return new CachedPrediction(prediction, clientId);
                        } catch (AggregateException ex) {
                            Debug.LogWarning($"Function: Failed to embed {tag} predictor with error: {ex.InnerException}. Predictions with this predictor will likely fail at runtime.");
                            return null;
                        }
                    }));
                    return predictions;
                })
                .Where(pred => pred != null)
                .ToList();
            settings.cache = cache;
            AndroidBuildHandler.cache = cache;
            return settings;
        }

        void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject (string projectPath) {
            if (cache == null)
                return;
            foreach (var prediction in cache) {
                // Check
                var arch = prediction.clientId.Replace("android-", string.Empty).Replace(":", string.Empty);
                var libDir = Path.Combine(projectPath, @"src", @"main", @"jniLibs", arch);
                if (!Directory.Exists(libDir))
                    continue;
                // Fetch resources
                var client = new DotNetClient(Function.URL);
                foreach (var resource in prediction.resources) {
                    try {
                        if (resource.type != @"dso")
                            continue;
                        var baseName = Path.GetFileName(PredictionService.GetResourcePath(resource, libDir));
                        var libName = $"lib{baseName}.so";
                        var path = Path.Combine(libDir, libName);
                        using var dsoStream = Task.Run(() => client.Download(resource.url)).Result;
                        using var fileStream = File.Create(path);
                        dsoStream.CopyTo(fileStream);
                    } catch (AggregateException ex) {
                        Debug.LogWarning($"Function: Failed to embed prediction resource for {prediction.tag} predictor with error: {ex.InnerException}. Predictions with this predictor will likely fail at runtime.");
                    }
                }
            }
            cache = null;
        }
    }
}