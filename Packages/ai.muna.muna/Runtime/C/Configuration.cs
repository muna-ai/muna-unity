/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.C {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using static Function;
    using Acceleration = Types.Acceleration;

    public sealed class Configuration : IDisposable {

        #region --Client API--

        public string tag {
            get {
                var sb = new StringBuilder(2048);
                configuration.GetConfigurationTag(sb, sb.Capacity).Throw();
                return sb.ToString();
            }
            set => configuration.SetConfigurationTag(value).Throw();
        }

        public string token {
            get {
                var sb = new StringBuilder(2048);
                configuration.GetConfigurationToken(sb, sb.Capacity).Throw();
                return sb.ToString();
            }
            set => configuration.SetConfigurationToken(value).Throw();
        }

        public Acceleration acceleration {
            get => configuration.GetConfigurationAcceleration(out var acceleration).Throw() == Status.Ok ? acceleration : default;
            set => configuration.SetConfigurationAcceleration(value).Throw();
        }

        public IntPtr device {
            get => configuration.GetConfigurationDevice(out var device).Throw() == Status.Ok ? device : default;
            set => configuration.SetConfigurationDevice(value).Throw();
        }

        public static string ConfigurationId {
            get {
                var sb = new StringBuilder(2048);
                GetConfigurationUniqueID(sb, sb.Capacity).Throw();
                return sb.ToString();
            }
        }

        public static string ClientId {
            get {
                var sb = new StringBuilder(64);
                GetConfigurationClientID(sb, sb.Capacity).Throw();
                return sb.ToString();
            }
        }

        public static Task InitializationTask {
            get {
                #if UNITY_WEBGL && !UNITY_EDITOR
                var tcs = new TaskCompletionSource<bool>();
                var context = GCHandle.Alloc(tcs, GCHandleType.Normal);
                SetInitializationHandler(OnFunctionInitialized, (IntPtr)context);
                return tcs.Task;
                [DllImport(Function.Assembly, EntryPoint = @"FXNSetInitializationHandler")]
                static extern Status SetInitializationHandler (Action<IntPtr> handler, IntPtr context);
                #else
                return Task.CompletedTask;
                #endif
            }
        }

        public Configuration () {
            CreateConfiguration(out var configuration).Throw();
            this.configuration = configuration;
        }

        public Task AddResource (string type, string path) {
            #if UNITY_WEBGL && !UNITY_EDITOR
            var tcs = new TaskCompletionSource<bool>();
            var context = GCHandle.Alloc(tcs, GCHandleType.Normal);
            AddConfigurationResource(configuration, type, path, OnAddConfigurationResource, (IntPtr)context);
            return tcs.Task;
            [DllImport(Function.Assembly, EntryPoint = @"FXNConfigurationAddResourceAsync")]
            static extern Status AddConfigurationResource (
                IntPtr configuration,
                [MarshalAs(UnmanagedType.LPUTF8Str)] string type,
                [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
                Action<IntPtr, Status> handler,
                IntPtr context
            );
            #else
            try {
                configuration.AddConfigurationResource(type, path).Throw();
                return Task.CompletedTask;
            } catch (Exception ex) {
                return Task.FromException(ex);
            }
            #endif
        }

        public void Dispose () => configuration.ReleaseConfiguration();
        #endregion


        #region --Operations--
        private readonly IntPtr configuration;

        public static implicit operator IntPtr (Configuration configuration) => configuration.configuration;

        [MonoPInvokeCallback(typeof(Action<IntPtr>))]
        private static void OnFunctionInitialized (IntPtr context) {
            var handle = (GCHandle)context;
            var tcs = handle.Target as TaskCompletionSource<bool>;
            handle.Free();
            tcs?.SetResult(true);
        }

        [MonoPInvokeCallback(typeof(Action<IntPtr, Status>))]
        private static void OnAddConfigurationResource (IntPtr context, Status status) {
            var handle = (GCHandle)context;
            var tcs = handle.Target as TaskCompletionSource<bool>;
            handle.Free();
            try {
                status.Throw();
                tcs?.SetResult(true);
            } catch (Exception ex) {
                tcs?.SetException(ex);
            }
        }
        #endregion
    }
}