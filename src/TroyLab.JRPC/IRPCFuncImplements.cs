using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TroyLab.JRPC
{
    internal interface IRPCFunc
    {
        Task<RPCResponse> ExecuteAsync(RPCRequest request);
    }

    internal class RPCFuncInputReply<TInput, TReply> : IRPCFunc
    {
        readonly Func<TInput, Task<TReply>> funcInput;
        readonly Func<TInput, RPCRequest, Task<TReply>> funcInputWithRPCRequest;

        public RPCFuncInputReply(Func<TInput, Task<TReply>> func)
        {
            this.funcInput = func;
        }

        public RPCFuncInputReply(Func<TInput, RPCRequest, Task<TReply>> func)
        {
            this.funcInputWithRPCRequest = func;
        }

        public async Task<RPCResponse> ExecuteAsync(RPCRequest request)
        {
            var reply = new RPCResponse();

            if (funcInputWithRPCRequest != null)
            {
                reply.Pack(await funcInputWithRPCRequest.Invoke(request.Unpack<TInput>(), request));
            }
            else
            {
                reply.Pack(await funcInput.Invoke(request.Unpack<TInput>()));
            }

            return reply;
        }
    }

    internal class RPCFuncInputNoReply<TInput> : IRPCFunc
    {
        readonly Func<TInput, Task> funcInput;
        readonly Func<TInput, RPCRequest, Task> funcInputWithRPCRequest;

        public RPCFuncInputNoReply(Func<TInput, Task> func)
        {
            this.funcInput = func;
        }

        public RPCFuncInputNoReply(Func<TInput, RPCRequest, Task> func)
        {
            this.funcInputWithRPCRequest = func;
        }

        public async Task<RPCResponse> ExecuteAsync(RPCRequest request)
        {
            var reply = new RPCResponse();

            if (funcInputWithRPCRequest != null)
            {
                await funcInputWithRPCRequest.Invoke(request.Unpack<TInput>(), request);
            }
            else
            {
                await funcInput.Invoke(request.Unpack<TInput>());
            }

            reply.Pack(new object());

            return reply;
        }
    }

    internal class RPCFuncNoInputNoReply : IRPCFunc
    {
        readonly Func<Task> funcNoInput;
        readonly Func<RPCRequest, Task> funcRPCRequest;

        public RPCFuncNoInputNoReply(Func<Task> func)
        {
            funcNoInput = func;
        }

        public RPCFuncNoInputNoReply(Func<RPCRequest, Task> func)
        {
            funcRPCRequest = func;
        }

        public async Task<RPCResponse> ExecuteAsync(RPCRequest request)
        {
            var reply = new RPCResponse();

            if (funcNoInput != null)
            {
                await funcNoInput.Invoke();
            }
            else
            {
                await funcRPCRequest.Invoke(request);
            }

            reply.Pack(new object());

            return reply;
        }
    }

    internal class RPCFuncNoInputReply<TReply> : IRPCFunc
    {
        readonly Func<Task<TReply>> funcNoInput;
        readonly Func<RPCRequest, Task<TReply>> funcRPCRequest;

        public RPCFuncNoInputReply(Func<Task<TReply>> func)
        {
            this.funcNoInput = func;
        }

        public RPCFuncNoInputReply(Func<RPCRequest, Task<TReply>> func)
        {
            this.funcRPCRequest = func;
        }

        public async Task<RPCResponse> ExecuteAsync(RPCRequest request)
        {
            var reply = new RPCResponse();

            if (funcNoInput != null)
            {
                reply.Pack(await funcNoInput.Invoke());
            }
            else
            {
                reply.Pack(await funcRPCRequest.Invoke(request));
            }

            return reply;
        }
    }
}
