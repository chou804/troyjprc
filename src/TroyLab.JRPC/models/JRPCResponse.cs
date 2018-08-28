using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TroyLab.JRPC
{
    public class JRPCResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; } = "2.0";
        [JsonProperty("result")]
        public JRaw Result { get; set; }
        [JsonProperty("error")]
        public JRPCError Error { get; set; }
    }
}
