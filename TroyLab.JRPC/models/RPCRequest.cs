using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    public class RPCRequest
    {
        public string PackedData;
        public string RPCFuncName;
        public string AccessToken;
        //public string JWT { get; set; }
        public string Account { get; set; }
        //public HttpContext Ctx { get; set; }

        public void Pack(Object obj)
        {
            RPCFuncName = ServiceNameHelper.ComposeFuncName(obj.GetType());
            PackedData = RPCPacker.Pack(obj);
        }

        public T Unpack<T>()
        {
            return RPCPacker.Unpack<T>(PackedData);
        }
    }
}
