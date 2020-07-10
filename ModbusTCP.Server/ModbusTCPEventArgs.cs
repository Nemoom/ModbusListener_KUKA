using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Utility;
using System.Collections.ObjectModel;

namespace ModbusTCP.Server
{
    public class ModbusTCPRequestEventArgs : EventArgs
    {
        public byte FunctionCode;
        public byte[] byte_StartAddress;
        public byte[] byte_Data;
        public ModbusTCPRequestEventArgs(byte _FunctionCode, byte[] _StartAddress, byte[] _Data)
        {
            FunctionCode = _FunctionCode;
            byte_StartAddress = _StartAddress;
            byte_Data = _Data;
        }
    }

    public class ModbusTCPDataEventArgs : EventArgs
    {
        public DiscriminatedUnion<ReadOnlyCollection<bool>, ReadOnlyCollection<ushort>> Data { get; set; }
        public RegisterType ModbusTCPDataType;
        public ushort StartAddress;
    }
}
