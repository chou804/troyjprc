using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TroyLab.JRPC;

namespace DemoApp.Services
{
    public class EchoService
    {
        //[JRPCMethod("Echo")]
        //public async Task<DEMO.EchoView> Echo(DEMO.Echo input)
        //{
        //    var v = new DEMO.EchoView { Msg = input.Msg };

        //    return await Task.FromResult(v);
        //}

        //[JRPCMethod(nameof(Func11_1))]
        //public Task<string> Func11_1(string input, RPCRequest req)
        //{
        //    //ok
        //    return Task.FromResult(nameof(Func11_1));
        //}

        //[JRPCMethod(nameof(Func10_1))]
        //public Task<string> Func10_1(string input)
        //{
        //    //ok
        //    return Task.FromResult(nameof(Func10_1));
        //}

        //[JRPCMethod(nameof(Func01_1))]
        //public Task<string> Func01_1(RPCRequest req)
        //{
        //    //ok
        //    return Task.FromResult(nameof(Func01_1));
        //}

        //[JRPCMethod(nameof(Func00_1))]
        //public Task<string> Func00_1()
        //{
        //    //ok
        //    return Task.FromResult(nameof(Func00_1));
        //}

        ////---------------------

        //[JRPCMethod(nameof(Func11_0))]
        //public async Task Func11_0(string input, RPCRequest req)
        //{
        //    //ok
        //    Console.WriteLine("TEST ok");
        //}

        //[JRPCMethod(nameof(Func10_0))]
        //public async Task Func10_0(string input)
        //{
        //    //ok
        //    Console.WriteLine("TEST OK");
        //}

        [JRPCMethod(nameof(Func01_0))]
        public Task Func01_0(RPCRequest req)
        {
            //ok
            return Task.FromResult(nameof(Func01_0));
        }

        //[JRPCMethod(nameof(Func00_0))]
        //public async Task Func00_0()
        //{
        //    //ok
        //    Console.WriteLine("test OK");
        //}
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
