using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;

namespace Network.Shared {
    public struct NetworkGuid : INetworkSerializable, IEquatable<NetworkGuid> {
        private FixedBytes16 _data;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.IsReader) {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out _data.byte0000);
                reader.ReadValueSafe(out _data.byte0001);
                reader.ReadValueSafe(out _data.byte0002);
                reader.ReadValueSafe(out _data.byte0003);
                reader.ReadValueSafe(out _data.byte0004);
                reader.ReadValueSafe(out _data.byte0005);
                reader.ReadValueSafe(out _data.byte0006);
                reader.ReadValueSafe(out _data.byte0007);
                reader.ReadValueSafe(out _data.byte0008);
                reader.ReadValueSafe(out _data.byte0009);
                reader.ReadValueSafe(out _data.byte0010);
                reader.ReadValueSafe(out _data.byte0011);
                reader.ReadValueSafe(out _data.byte0012);
                reader.ReadValueSafe(out _data.byte0013);
                reader.ReadValueSafe(out _data.byte0014);
                reader.ReadValueSafe(out _data.byte0015);
            } else {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_data.byte0000);
                writer.WriteValueSafe(_data.byte0001);
                writer.WriteValueSafe(_data.byte0002);
                writer.WriteValueSafe(_data.byte0003);
                writer.WriteValueSafe(_data.byte0004);
                writer.WriteValueSafe(_data.byte0005);
                writer.WriteValueSafe(_data.byte0006);
                writer.WriteValueSafe(_data.byte0007);
                writer.WriteValueSafe(_data.byte0008);
                writer.WriteValueSafe(_data.byte0009);
                writer.WriteValueSafe(_data.byte0010);
                writer.WriteValueSafe(_data.byte0011);
                writer.WriteValueSafe(_data.byte0012);
                writer.WriteValueSafe(_data.byte0013);
                writer.WriteValueSafe(_data.byte0014);
                writer.WriteValueSafe(_data.byte0015);
            }
        }

        public override string ToString() {
            return _data.ToString();
        }

        private static byte[] ToArray(NetworkGuid networkGuid) {
            return new[] {
                networkGuid._data.byte0000, networkGuid._data.byte0001, networkGuid._data.byte0002,
                networkGuid._data.byte0003, networkGuid._data.byte0004, networkGuid._data.byte0005,
                networkGuid._data.byte0006, networkGuid._data.byte0007, networkGuid._data.byte0008,
                networkGuid._data.byte0009, networkGuid._data.byte0010, networkGuid._data.byte0011,
                networkGuid._data.byte0012, networkGuid._data.byte0013, networkGuid._data.byte0014,
                networkGuid._data.byte0015
            };
        }

        private static FixedBytes16 ReadDataFromArray(byte[] dataArray) {
            FixedBytes16 localData = new FixedBytes16();
            localData.byte0000 = dataArray[0];
            localData.byte0001 = dataArray[1];
            localData.byte0002 = dataArray[2];
            localData.byte0003 = dataArray[3];
            localData.byte0004 = dataArray[4];
            localData.byte0005 = dataArray[5];
            localData.byte0006 = dataArray[6];
            localData.byte0007 = dataArray[7];
            localData.byte0008 = dataArray[8];
            localData.byte0009 = dataArray[9];
            localData.byte0010 = dataArray[10];
            localData.byte0011 = dataArray[11];
            localData.byte0012 = dataArray[12];
            localData.byte0013 = dataArray[13];
            localData.byte0014 = dataArray[14];
            localData.byte0015 = dataArray[15];
            return localData;
        }

        public static implicit operator Guid(NetworkGuid g) => new Guid(ToArray(g));
        public static implicit operator NetworkGuid(Guid s) => new NetworkGuid() { _data = ReadDataFromArray(s.ToByteArray()) };

        public bool Equals(NetworkGuid other) {
            return _data.Equals(other._data);
        }

        public override bool Equals(object obj) {
            return obj is NetworkGuid other && Equals(other);
        }

        public override int GetHashCode() {
            return _data.GetHashCode();
        }
    }
}