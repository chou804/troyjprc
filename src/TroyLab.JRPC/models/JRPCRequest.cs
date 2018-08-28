using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TroyLab.JRPC
{
    /*
    {
      "jsonrpc": "2.0",
      "method": "Main.Echo",
      "params": {
        "name": "John Doe"
      },
      "id": "243a718a-2ebb-4e32-8cc8-210c39e8a14b"
    }
    */
    public class JRPCRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; } = "2.0";
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("params")]
        public JRaw Params { get; set; }
    }
}
