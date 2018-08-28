using System;

namespace TroyLab.JRPC
{
    public class ServiceNameHelper
    {
        static readonly string HOST_NAME = System.Net.Dns.GetHostName();

        public static string ComposeServiceName(string fnName)
        {
            return $"RPC-{fnName.Replace(".", "_")}";
        }

        public static string ComposeServiceId(string fnName, int port)
        {
            return $"{HOST_NAME}-RPC-{fnName.Replace(".", "_")}:{port}";
        }

        public static string ComposeFuncName(Type type)
        {
            var methodName = $"{type.DeclaringType.Name}.{type.Name}";

            return methodName;
        }
    }
}
