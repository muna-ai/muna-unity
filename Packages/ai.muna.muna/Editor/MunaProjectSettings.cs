/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyCompany(@"NatML Inc.")]
[assembly: AssemblyTitle(@"Muna.Editor")]
[assembly: AssemblyVersion(Muna.Muna.Version)]
[assembly: AssemblyCopyright(@"Copyright © 2025 NatML Inc. All Rights Reserved.")]
[assembly: InternalsVisibleTo(@"Muna.Tests.Editor")]
[assembly: InternalsVisibleTo(@"Muna.Tests.Runtime")]

namespace Muna.Editor {

    using UnityEditor;
    using Internal;

    /// <summary>
    /// Muna settings for the current Unity project.
    /// </summary>
    [FilePath(@"ProjectSettings/Muna.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class MunaProjectSettings : ScriptableSingleton<MunaProjectSettings> {

        #region --Client API--
        public string accessKey;

        public void Save() => Save(false);
        #endregion


        #region --Operations--

        [InitializeOnLoadMethod]
        private static void OnLoad() => MunaSettings.Instance = MunaSettings.Create(instance.accessKey);

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options) => MunaSettings.Instance = MunaSettings.Create(instance.accessKey);
        #endregion
    }
}