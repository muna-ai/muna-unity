/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyCompany(@"NatML Inc.")]
[assembly: AssemblyTitle(@"Function.Editor")]
[assembly: AssemblyVersion(Function.Function.Version)]
[assembly: AssemblyCopyright(@"Copyright © 2025 NatML Inc. All Rights Reserved.")]
[assembly: InternalsVisibleTo(@"Function.Tests.Editor")]
[assembly: InternalsVisibleTo(@"Function.Tests.Runtime")]

namespace Function.Editor {

    using UnityEditor;
    using Internal;

    /// <summary>
    /// Function settings for the current Unity project.
    /// </summary>
    [FilePath(@"ProjectSettings/Function.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class FunctionProjectSettings : ScriptableSingleton<FunctionProjectSettings> {

        #region --Client API--
        public string accessKey;

        public void Save () => Save(false);
        #endregion


        #region --Operations--

        [InitializeOnLoadMethod]
        private static void OnLoad () => FunctionSettings.Instance = FunctionSettings.Create(instance.accessKey);

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor (EnterPlayModeOptions options) => FunctionSettings.Instance = FunctionSettings.Create(instance.accessKey);
        #endregion
    }
}