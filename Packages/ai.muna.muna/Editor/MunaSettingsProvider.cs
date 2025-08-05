/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Editor {

    using System.Collections.Generic;
    using UnityEditor;

    internal static class MunaSettingsProvider {

        [SettingsProvider]
        public static SettingsProvider CreateProvider () => new SettingsProvider(@"Project/Muna", SettingsScope.Project) {
            label = @"Muna",
            guiHandler = searchContext => {
                var settings = MunaProjectSettings.instance;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField(@"Muna Account", EditorStyles.boldLabel);
                settings.accessKey = EditorGUILayout.TextField(@"Access Key", settings.accessKey);
                if (EditorGUI.EndChangeCheck())
                    settings.Save();
            },
            keywords = new HashSet<string>(new[] { @"Muna", @"Function", @"NatML" }),
        };
    }
}
