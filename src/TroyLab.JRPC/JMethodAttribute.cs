using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class JRPCMethodAttribute : Attribute
    {
        public string Name { get; }

        //public JRPCMethodAttribute() { }
        public JRPCMethodAttribute(string name)
        {
            Name = name.ToLower();
        }
    }
}
