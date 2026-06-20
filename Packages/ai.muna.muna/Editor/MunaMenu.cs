/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

namespace Muna.Editor {

    using System.IO;
    using UnityEditor;
    using Internal;

    internal static class MunaMenu {

        private const int BasePriority = -50;

        [MenuItem(@"Tools/Muna/Muna " + Muna.Version, false, BasePriority)]
        private static void Version() { }

        [MenuItem(@"Tools/Muna/Muna " + Muna.Version, true, BasePriority)]
        private static bool EnableVersion() => false;

        [MenuItem(@"Tools/Muna/Get Access Key", false, BasePriority + 1)]
        private static void GetAccessKey() => Help.BrowseURL(@"https://muna.ai/settings/developer");

        [MenuItem(@"Tools/Muna/Explore Predictors", false, BasePriority + 2)]
        private static void OpenExplore() => Help.BrowseURL(@"https://muna.ai/explore");

        [MenuItem(@"Tools/Muna/View the Docs", false, BasePriority + 3)]
        private static void OpenDocs() => Help.BrowseURL(@"https://docs.muna.ai");

        [MenuItem(@"Tools/Muna/Report an Issue", false, BasePriority + 4)]
        private static void ReportIssue() => Help.BrowseURL(@"https://github.com/muna-ai/muna-unity");
    }
}
