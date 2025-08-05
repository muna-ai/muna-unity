/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using API;
    using Types;
    using FunctionSettings = Internal.FunctionSettings;

#if UNITY_IOS || UNITY_VISIONOS
    using UnityEditor.iOS.Xcode;
    using UnityEditor.iOS.Xcode.Extensions;
#endif

    internal sealed class iOSBuildHandler : BuildHandler, IPostprocessBuildWithReport {

        private List<CachedPrediction> cache;
        private static readonly Dictionary<BuildTarget, string> ClientIds = new () {
            [BuildTarget.iOS] = @"ios-arm64",
            [BuildTarget.VisionOS] = @"visionos-arm64"
        };

        protected override BuildTarget[] targets => ClientIds.Keys.ToArray();

        protected override FunctionSettings CreateSettings (BuildReport report) {
            var projectSettings = FunctionProjectSettings.instance;
            var settings = FunctionSettings.Create(projectSettings.accessKey);
            var embeds = GetEmbeds();
            var cache = new List<CachedPrediction>();
            var clientId = ClientIds[report.summary.platform];
            foreach (var embed in embeds) {
                var client = new DotNetClient(embed.url, embed.accessKey);
                var fxn = new Function(client);
                var predictions = embed.tags
                    .Select(tag => {
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
                    })
                    .Where(pred => pred != null)
                    .ToArray();
                cache.AddRange(predictions);
            }
            settings.cache = cache;
            this.cache = cache;
            return settings;
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) {
            if (!targets.Contains(report.summary.platform))
                return;
            if (cache == null)
                return;
            var frameworkDir = Path.Combine(report.summary.outputPath, @"Frameworks", @"Function");
            Directory.CreateDirectory(frameworkDir);
            var client = new DotNetClient(Function.URL);
            var frameworks = new List<string>();
            foreach (var prediction in cache)
                foreach (var resource in prediction.resources) {
                    try {
                        if (resource.type != @"dso")
                            continue;
                        var dsoPath = Path.GetTempFileName();
                        {
                            using var dsoStream = Task.Run(() => client.Download(resource.url)).Result;
                            using var fileStream = File.Create(dsoPath);
                            dsoStream.CopyTo(fileStream);
                        }
                        ZipFile.ExtractToDirectory(dsoPath, frameworkDir, true);
                        frameworks.Add(resource.name);
                    } catch (AggregateException ex) {
                        Debug.LogWarning($"Function: Failed to embed prediction resource for {prediction.tag} predictor with error: {ex.InnerException}. Predictions with this predictor will likely fail at runtime.");
                    }
                }
        #if UNITY_IOS || UNITY_VISIONOS
            var xcodeProjectName = report.summary.platform == BuildTarget.VisionOS ?
                @"Unity-VisionOS.xcodeproj" : // Unity needs to fix `PBXProject::GetPBXProjectPath`
                @"Unity-iPhone.xcodeproj";
            var pbxPath = Path.Combine(
                report.summary.outputPath,
                xcodeProjectName,
                @"project.pbxproj"
            );
            var project = new PBXProject();
            project.ReadFromFile(pbxPath);
            var targetGuid = project.GetUnityMainTargetGuid();
            foreach (var framework in frameworks) {
                var frameworkGuid = project.AddFile(
                    @"Frameworks/Function/" + framework,
                    @"Frameworks/" + framework,
                    PBXSourceTree.Source
                );
                project.AddFileToEmbedFrameworks(targetGuid, frameworkGuid);
            }
            project.WriteToFile(pbxPath);
        #endif
            cache = null;
        }
    }
}