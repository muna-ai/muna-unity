/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Muna.Beta.OpenAI {

    /// <summary>
    /// Binary data with an optional content type.
    /// </summary>
    public class BinaryData {

        public bool IsEmpty => data.Length == 0;
        public int Length => data.Length;
        public string? MediaType { get; private set; }
        private readonly byte[] data;

        public BinaryData(byte[] data, string mediaType) {
            this.data = data;
            this.MediaType = mediaType;
        }

        public byte[] ToArray() => this.data;
    }
}