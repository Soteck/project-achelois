using System;
using Unity.Collections;
using Unity.Netcode;

namespace Network.Shared {
    public struct NetworkString : INetworkSerializable, IEquatable<NetworkString>
    {
        private FixedString32Bytes _info;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _info);
        }

        public override string ToString()
        {
            return _info.ToString();
        }

        public static implicit operator string(NetworkString s) => s.ToString();
        public static implicit operator NetworkString(string s) => new NetworkString() { _info = new FixedString32Bytes(s) };

        public bool Equals(NetworkString other) {
            return _info.Equals(other._info);
        }

        public override bool Equals(object obj) {
            return obj is NetworkString other && Equals(other);
        }

        public override int GetHashCode() {
            return _info.GetHashCode();
        }
    }
}
