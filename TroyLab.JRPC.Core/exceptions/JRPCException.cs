using System;

namespace TroyLab.JRPC.Core
{
    public class JRPCException : Exception
    {
        public int HttpStatusCode { get; set; } = 400;

        public JRPCException() { }
        public JRPCException(string message) : base(message) { }
        public JRPCException(string message, Exception inner) : base(message, inner) { }

        public JRPCException(string message, int httpStatusCode) : base(message) { HttpStatusCode = httpStatusCode; }
        public JRPCException(string message, Exception inner, int httpStatusCode) : base(message, inner) { HttpStatusCode = httpStatusCode; }
    }
}
