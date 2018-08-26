using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    /*
     * RPCFunc 有放 Authorized 表示要登入才能使用, 沒有放的表示可匿名存取
     */



    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class JRPCAuthorizedAttribute : Attribute
    {
        public readonly string Resource;
        public readonly string[] Actions;

        /// <summary>
        /// 沒有標注任何 resource, 表示只要登入就可以呼叫
        /// </summary>
        public JRPCAuthorizedAttribute() { }

        public JRPCAuthorizedAttribute(string resource, params string[] actions)
        {
            Resource = resource;
            Actions = actions;
        }
    }
}
