/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

using FunctionClient = Function.Function;
using EmbedAttribute = Function.Function.EmbedAttribute;

namespace Function.Editor.Build {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using Internal;

    internal abstract class BuildHandler : IPreprocessBuildWithReport {
    
        #region --Client API--
        internal struct Embed {
            public string url;
            public string? accessKey;
            public string[] tags;
        }

        protected abstract BuildTarget[] targets { get; }
        public virtual int callbackOrder => -1_000_000; // run very early, but not too early ;)

        protected abstract FunctionSettings CreateSettings (BuildReport report);

        internal static Embed[] GetEmbeds () {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(assembly => assembly.GetTypes()).ToArray();
            var defaultEmbeds = types
                .SelectMany(type => Attribute.GetCustomAttributes(type, typeof(EmbedAttribute)))
                .Cast<EmbedAttribute>()
                .Select(embed => new Embed {
                    url = FunctionClient.URL,
                    accessKey = FunctionProjectSettings.instance.accessKey,
                    tags = embed.tags
                })
                .ToArray();
            var customEmbeds = types
                .SelectMany(type => { // thanks Jamie!
                    try {
                        return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    }
                    catch (Exception ex) {
                        Debug.LogWarning($"Function: Failed to inspect type {type} for predictor embeds with exception: {ex.Message}. Predictions might fail at runtime.");
                        return new PropertyInfo[0];
                    }
                })
                .Where(property =>
                    Attribute.IsDefined(property, typeof(EmbedAttribute)) &&
                    property.PropertyType == typeof(FunctionClient)
                )
                .Select(property  => {
                    var attribute = property.GetCustomAttribute<EmbedAttribute>();
                    var getter = CreateDelegateForProperty<FunctionClient>(property);
                    var fxn = getter!();
                    return new Embed {
                        url = fxn.client.url,
                        accessKey = fxn.client.accessKey,
                        tags = attribute.tags
                    };
                })
                .ToArray();
            return Enumerable.Concat(defaultEmbeds, customEmbeds).ToArray();
        }
        #endregion


        #region --Operations--
        protected const string CachePath = @"Assets/__FXN_DELETE_THIS__";

        void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
            if (!targets.Contains(report.summary.platform))
                return;
            var settings = CreateSettings(report);
            EditorApplication.update += FailureListener;
            ClearSettings();
            EmbedSettings(settings);
        }

        private void FailureListener () {
            if (BuildPipeline.isBuildingPlayer)
                return;
            ClearSettings();
            EditorApplication.update -= FailureListener;
        }
        #endregion


        #region --Utilities--

        private static void EmbedSettings (FunctionSettings settings) {
            Directory.CreateDirectory(CachePath);
            AssetDatabase.CreateAsset(settings, $"{CachePath}/Function.asset");
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList() ?? new List<UnityEngine.Object>();
            assets.Add(settings);
            PlayerSettings.SetPreloadedAssets(assets.ToArray());
        }

        private static void ClearSettings () {
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList();
            if (assets != null) {
                assets.RemoveAll(asset => asset && asset.GetType() == typeof(FunctionSettings));
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
            AssetDatabase.DeleteAsset(CachePath);
        }

        private static Func<T>? CreateDelegateForProperty<T> (PropertyInfo property) {
            var getter = property.GetGetMethod(true);
            return getter != null && getter.ReturnType == typeof(T) ?
                (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), getter) :
                null;
        }
        #endregion
    }
}