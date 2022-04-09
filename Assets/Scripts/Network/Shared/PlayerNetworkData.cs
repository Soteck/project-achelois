using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Network.Shared {
    public struct PlayerNetworkData : INetworkSerializable, IEquatable<PlayerNetworkData> {
        [SerializeField] public FixedString32Bytes playerName;

        [SerializeField] public ulong clientId;


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref clientId);
        }

        public bool Equals(PlayerNetworkData other) {
            return playerName.Equals(other.playerName) && clientId == other.clientId;
        }

        public override bool Equals(object obj) {
            return obj is PlayerNetworkData other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (playerName.GetHashCode() * 397) ^ clientId.GetHashCode();
            }
        }
    }
}