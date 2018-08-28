using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    public class RPCResponse
    {
        public string PackedData;

        public void Pack(object obj)
        {
            PackedData = RPCPacker.Pack(obj);
        }

        public T Unpack<T>()
        {
            return RPCPacker.Unpack<T>(PackedData);
        }
    }
}
