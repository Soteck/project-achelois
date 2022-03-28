using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Network.Shared {
    public struct EquipableItemNetworkData  : INetworkSerializable, IEquatable<EquipableItemNetworkData> {
        
        [SerializeField]
        public FixedString32Bytes itemID;
        
        [SerializeField]
        public FixedString32Bytes itemMeta;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref itemID);
            serializer.SerializeValue(ref itemMeta);
        }
        

        public bool Equals(EquipableItemNetworkData other) {
            return itemID.Equals(other.itemID) && itemMeta.Equals(other.itemMeta);
        }

        public override bool Equals(object obj) {
            return obj is EquipableItemNetworkData other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (itemID.GetHashCode() * 397) ^ itemMeta.GetHashCode();
            }
        }
    }
}