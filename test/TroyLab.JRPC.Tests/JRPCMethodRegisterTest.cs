using System;
using Xunit;
using TroyLab.JRPC;
using System.Threading.Tasks;

namespace TroyLab.JRPC.Tests
{
    public class JRPCMethodRegisterTest
    {
        [Fact]
        public void JRPCMethodRegister_Test()
        {
            RPCServer.AddService(new DummyService());
        }
    }

    public class DummyService
    {
        [JRPCMethod(nameof(Func11_1))]
        public Task<string> Func11_1(string input, RPCRequest req)
        {
            return Task.FromResult(nameof(Func11_1));
        }

        [JRPCMethod(nameof(Func10_1))]
        public Task<string> Func10_1(string input)
        {
            return Task.FromResult(nameof(Func10_1));
        }

        [JRPCMethod(nameof(Func01_1))]
        public Task<string> Func01_1(RPCRequest req)
        {
            return Task.FromResult(nameof(Func01_1));
        }

        [JRPCMethod(nameof(Func00_1))]
        public Task<string> Func00_1()
        {
            return Task.FromResult(nameof(Func00_1));
        }

        //---------------------

        [JRPCMethod(nameof(Func11_0))]
        public Task Func11_0(string input, RPCRequest req)
        {
            return Task.FromResult(nameof(Func11_0));
        }

        [JRPCMethod(nameof(Func10_0))]
        public Task Func10_0(string input)
        {
            return Task.FromResult(nameof(Func10_0));
        }

        [JRPCMethod(nameof(Func01_0))]
        public Task Func01_0(RPCRequest req)
        {
            return Task.FromResult(nameof(Func01_0));
        }

        [JRPCMethod(nameof(Func00_0))]
        public Task Func00_0()
        {
            return Task.FromResult(nameof(Func00_0));
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
