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
        public async Task<DUMMY.EchoView> Echo11(DUMMY.Echo input)
        {
            var v = new DUMMY.EchoView { Msg = input.Msg };

            return await Task.FromResult(v);
        }
    }

    public class DUMMY
    {
        public class Echo
        {
            public string Msg { get; set; }
        }

        public class EchoView
        {
            public string Msg { get; set; }
        }

        public class GetUserInfo
        {
            public string UserName { get; set; }
        }

        public class GetUserInfoView
        {
            public string UserInfo { get; set; }
        }
    }
}
