/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using FunctionSettings = Internal.FunctionSettings;

    internal sealed class LinuxBuildHandler : BuildHandler {

        protected override BuildTarget[] targets => new [] {
            BuildTarget.StandaloneLinux64,
            BuildTarget.LinuxHeadlessSimulation,
            BuildTarget.EmbeddedLinux,
        };

        protected override FunctionSettings CreateSettings (BuildReport report) {
            var projectSettings = FunctionProjectSettings.instance;
            var settings = FunctionSettings.Create(projectSettings.accessKey);
            return settings;
        }
    }
}