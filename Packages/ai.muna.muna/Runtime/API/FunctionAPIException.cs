/*
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System;

    /// <summary>
    /// Function API exception.
    /// </summary>
    public sealed class FunctionAPIException : Exception {

        public readonly int status;

        public FunctionAPIException (string message, int status)  : base(message) => this.status = status;
    }
}