/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Editor {

    using System.Collections.Generic;
    using UnityEditor;

    internal static class FunctionSettingsProvider {

        [SettingsProvider]
        public static SettingsProvider CreateProvider () => new SettingsProvider(@"Project/Function", SettingsScope.Project) {
            label = @"Function",
            guiHandler = searchContext => {
                var settings = FunctionProjectSettings.instance;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField(@"Function Account", EditorStyles.boldLabel);
                settings.accessKey = EditorGUILayout.TextField(@"Access Key", settings.accessKey);
                if (EditorGUI.EndChangeCheck())
                    settings.Save();
            },
            keywords = new HashSet<string>(new[] { @"Function", @"NatML" }),
        };
    }
}
