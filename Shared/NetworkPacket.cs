using System;

namespace Network
{
    public class NetworkPacket
    {
        public Type DataType { get; set; }

        public byte[] Bytes { get; set; }
    }
}