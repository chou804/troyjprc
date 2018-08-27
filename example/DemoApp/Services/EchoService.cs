using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TroyLab.JRPC;

namespace DemoApp.Services
{
    public class EchoService
    {
        [JRPCMethod("Echo")]
        public async Task<EchoView> Echo(Echo input)
        {
            var v = new EchoView { Msg = input.Msg };

            return await Task.FromResult(v);
        }
    }

    public class Echo
    {
        public string Msg { get; set; }
    }

    public class EchoView
    {
        public string Msg { get; set; }
    }
}
