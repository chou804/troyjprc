using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TroyLab.JRPC;

namespace SimpleApp
{
    public class MoreServices
    {
        [JRPCMethod(nameof(Func11_1))]
        public Task<string> Func11_1(string input, RPCRequest req)
        {
            return Task.FromResult($"{input}_{req.RPCFuncName}");
        }

        [JRPCMethod(nameof(Func10_1))]
        public Task<string> Func10_1(string input)
        {
            return Task.FromResult(input);
        }

        [JRPCMethod(nameof(Func01_1))]
        public Task<string> Func01_1(RPCRequest req)
        {
            return Task.FromResult(req.RPCFuncName);
        }

        [JRPCMethod(nameof(Func00_1))]
        public Task<string> Func00_1()
        {
            return Task.FromResult(nameof(Func00_1));
        }

        [JRPCMethod(nameof(Func11_0))]
        public async Task Func11_0(string input, RPCRequest req)
        {
            Console.WriteLine(input);
        }

        [JRPCMethod(nameof(Func10_0))]
        public async Task Func10_0(string input)
        {
            Console.WriteLine(input);
        }

        [JRPCMethod(nameof(Func01_0))]
        public Task Func01_0(RPCRequest req)
        {
            return Task.FromResult(req.RPCFuncName);
        }

        [JRPCMethod(nameof(Func00_0))]
        public async Task Func00_0()
        {
            Console.WriteLine(nameof(Func00_0));
        }
    }
}
