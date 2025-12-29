/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.C {

    using System;
    using static Function;

    public sealed class ValueList : IDisposable {

        #region --Client API--

        public Value this[int index] {
            get {
                list.GetValueListValue(index, out var value).Throw();
                return new Value(value);
            }
        }

        public int size => list.GetValueListSize(out var size).Throw() == Status.Ok ? size : default;

        public void Add(Value value) => list.AppendValueListValue(value).Throw();

        public void Dispose () => list.ReleaseValue();
        #endregion


        #region --Operations--
        private readonly IntPtr list;

        internal ValueList(IntPtr list) => this.list = list;

        public static implicit operator IntPtr(ValueList list) => list.list;
        #endregion
    }
}