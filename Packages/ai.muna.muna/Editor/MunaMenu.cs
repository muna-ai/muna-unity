/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Editor {

    using UnityEditor;

    internal static class MunaMenu {

        private const int BasePriority = -50;
        
        [MenuItem(@"Muna/Muna " + Muna.Version, false, BasePriority)]
        private static void Version() { }

        [MenuItem(@"Muna/Muna " + Muna.Version, true, BasePriority)]
        private static bool EnableVersion() => false;

        [MenuItem(@"Muna/Get Access Key", false, BasePriority + 1)]
        private static void GetAccessKey() => Help.BrowseURL(@"https://muna.ai/settings/developer");

        [MenuItem(@"Muna/Explore Predictors", false, BasePriority + 2)]
        private static void OpenExplore() => Help.BrowseURL(@"https://muna.ai/explore");

        [MenuItem(@"Muna/View the Docs", false, BasePriority + 3)]
        private static void OpenDocs() => Help.BrowseURL(@"https://docs.muna.ai");

        [MenuItem(@"Muna/Report an Issue", false, BasePriority + 4)]
        private static void ReportIssue() => Help.BrowseURL(@"https://github.com/muna-ai/muna-unity");
    }
}
