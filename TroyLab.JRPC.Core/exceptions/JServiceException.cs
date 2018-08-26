using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC.Core
{
    public class JServiceException : Exception
    {
        public JServiceException() { }
        public JServiceException(string message) : base(message) { }
        public JServiceException(string message, Exception inner) : base(message, inner) { }
    }
}
