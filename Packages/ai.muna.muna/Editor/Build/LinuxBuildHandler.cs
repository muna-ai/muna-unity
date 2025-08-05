/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Editor.Build {

    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using MunaSettings = Internal.MunaSettings;

    internal sealed class LinuxBuildHandler : BuildHandler {

        protected override BuildTarget[] targets => new [] {
            BuildTarget.StandaloneLinux64,
            BuildTarget.LinuxHeadlessSimulation,
            BuildTarget.EmbeddedLinux,
        };

        protected override MunaSettings CreateSettings(BuildReport report) {
            var projectSettings = MunaProjectSettings.instance;
            var settings = MunaSettings.Create(projectSettings.accessKey);
            return settings;
        }
    }
}