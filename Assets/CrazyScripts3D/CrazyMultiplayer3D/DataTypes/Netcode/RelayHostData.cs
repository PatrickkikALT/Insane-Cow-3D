using System;
using UnityEngine;



namespace FirePixel.Networking
{
    [Tooltip("Data container for hosting a server")]
    public struct RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }
}