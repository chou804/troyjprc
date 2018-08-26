using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC.Core
{
    public class JRPCError
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public object Data { get; set; }

        public const int PARSE_ERROR = -32700;
        public const int INVALID_REQUEST = -32600;
        public const int METHOD_NOT_FOUND = -32601;
        public const int INVALID_PARAMS = -32602;
        public const int INTERNAL_ERROR = -32603;

        static Dictionary<int, string> codeMessageDict = new Dictionary<int, string>
        {
            {-32700, "Parse error"},
            {-32600, "Invalid Request"},
            {-32601, "Method not found"},
            {-32602, "Invalid params"},
            {-32603, "Internal error"}
        };

        public JRPCError() { }

        public JRPCError(int code, object data)
        {
            Code = code;
            Data = data;

            if (codeMessageDict.TryGetValue(code, out string message))
                Message = message;
        }

        public JRPCError(int code, string message, object data)
        {
            Code = code;
            Message = message;
            Data = data;
        }
    }
}
