using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC.Core
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class JRPCMethodAttribute : Attribute
    {
    }
}
