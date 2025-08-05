/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using FunctionSettings = Internal.FunctionSettings;

    internal sealed class WindowsBuildHandler : BuildHandler {

        protected override BuildTarget[] targets => new [] { BuildTarget.StandaloneWindows64 };

        protected override FunctionSettings CreateSettings (BuildReport report) {
            var projectSettings = FunctionProjectSettings.instance;
            var settings = FunctionSettings.Create(projectSettings.accessKey);
            return settings;
        }
    }
}