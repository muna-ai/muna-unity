/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.C {

    using System;
    using System.Text;
    using static Function;

    public sealed class ValueMap : IDisposable {

        #region --Client API--

        public Value this [string key] {
            get => GetValue(key);
            set => map.SetValueMapValue(key, value).Throw();
        }

        public int size => map.GetValueMapSize(out var size).Throw() == Status.Ok ? size : default;

        public ValueMap () : this(CreateValueMap(out var map).Throw() == Status.Ok ? map : default) { }

        public string GetKey (int index) {
            var key = new StringBuilder(2048);
            map.GetValueMapKey(index, key, key.Capacity).Throw();
            return key.ToString();
        }

        public Value GetValue (string key) {
            map.GetValueMapValue(key, out var value).Throw();
            return new Value(value);
        }

        public void Dispose () => map.ReleaseValueMap();
        #endregion


        #region --Operations--
        private readonly IntPtr map;

        internal ValueMap (IntPtr map) => this.map = map;

        public static implicit operator IntPtr (ValueMap map) => map.map;
        #endregion
    }
}