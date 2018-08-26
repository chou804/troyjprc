using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TroyLab.JRPC;

namespace DemoApp.Services
{
    public class EchoService
    {
        [JRPCMethod]
        public async Task<DEMO.EchoView> Echo(DEMO.Echo input)
        {
            var v = new DEMO.EchoView { Msg = input.Msg };

            return await Task.FromResult(v);
        }
    }

    public class DEMO
    {
        public class Echo
        {
            public string Msg { get; set; }
        }

        public class EchoView
        {
            public string Msg { get; set; }
        }
    }
}
