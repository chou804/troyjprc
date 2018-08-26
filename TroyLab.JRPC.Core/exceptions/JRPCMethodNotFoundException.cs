using System;

namespace TroyLab.JRPC.Core
{
    public class JRPCMethodNotFoundException : Exception
    {
        public JRPCMethodNotFoundException() { }
        public JRPCMethodNotFoundException(string message) : base(message) { }
        public JRPCMethodNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
