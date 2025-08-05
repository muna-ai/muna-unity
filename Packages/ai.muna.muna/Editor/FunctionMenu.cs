/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Editor {

    using UnityEditor;

    internal static class FunctionMenu {

        private const int BasePriority = -50;
        
        [MenuItem(@"Function/Function " + Function.Version, false, BasePriority)]
        private static void Version () { }

        [MenuItem(@"Function/Function " + Function.Version, true, BasePriority)]
        private static bool EnableVersion () => false;

        [MenuItem(@"Function/Get Access Key", false, BasePriority + 1)]
        private static void GetAccessKey () => Help.BrowseURL(@"https://fxn.ai/settings/developer");

        [MenuItem(@"Function/Explore Predictors", false, BasePriority + 2)]
        private static void OpenExplore () => Help.BrowseURL(@"https://fxn.ai/explore");

        [MenuItem(@"Function/View the Docs", false, BasePriority + 3)]
        private static void OpenDocs () => Help.BrowseURL(@"https://docs.fxn.ai");

        [MenuItem(@"Function/Report an Issue", false, BasePriority + 4)]
        private static void ReportIssue () => Help.BrowseURL(@"https://github.com/fxnai/fxn3d");
    }
}
