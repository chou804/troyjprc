using System;

namespace TroyLab.JRPC
{
    public class JRPCMethodNotFoundException : Exception
    {
        public JRPCMethodNotFoundException() { }
        public JRPCMethodNotFoundException(string message) : base(message) { }
        public JRPCMethodNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
