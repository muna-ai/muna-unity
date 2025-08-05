/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Editor.Build {

    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using FunctionSettings = Internal.FunctionSettings;

    internal sealed class WebGLBuildHandler : BuildHandler {

        protected override BuildTarget[] targets => new [] { BuildTarget.WebGL };
        private static string[] EM_ARGS => new [] {
            @"-Wl,-uFXN_WEBGL_INIT",
            @"-lembind",
            @"-sEXTRA_EXPORTED_RUNTIME_METHODS=FS",
        };

        protected override FunctionSettings CreateSettings (BuildReport report) {
            var configPath = Path.Combine(GetEmscriptenPath(), @"tools", @"config.py");
            if (File.Exists(configPath)) {
                var config = File.ReadAllText(configPath).TrimEnd();
                if (!Regex.IsMatch(config, @"FROZEN_CACHE\s*=\s*False\s*$", RegexOptions.Multiline))
                    File.WriteAllText(configPath, config + "\nFROZEN_CACHE = False");
            }
            PlayerSettings.WebGL.emscriptenArgs = GetEmscriptenArgs();
            var projectSettings = FunctionProjectSettings.instance;
            var settings = FunctionSettings.Create(projectSettings.accessKey);
            return settings;
        }

        private static string GetEmscriptenArgs () {
            var cleanedArgs = Regex.Replace(
                PlayerSettings.WebGL.emscriptenArgs,
                @"-Wl,-uFXN_WEBGL_PUSH.*?-Wl,-uFXN_WEBGL_POP",
                string.Empty,
                RegexOptions.Singleline
            ).Split(' ');
            var args = new List<string>();
            args.AddRange(cleanedArgs);
            args.Add(@"-Wl,-uFXN_WEBGL_PUSH");
            args.AddRange(EM_ARGS);
            args.Add(@"-Wl,-uFXN_WEBGL_POP");
            return string.Join(@" ", args);
        }

        private static string GetEmscriptenPath () {
            var rootDir = Path.GetDirectoryName(EditorApplication.applicationPath);
            var emccDir = Path.Combine(
                rootDir,
                @"PlaybackEngines",
                @"WebGLSupport",
                @"BuildTools",
                @"Emscripten",
                @"emscripten"
            );
            return emccDir;
        }
    }
}