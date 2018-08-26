using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TroyLab.JRPC
{
    //interface IRPCFunc { Task<RPCResponse> ExecuteAsync(RPCRequest request); }

    public class RPCServer
    {
        static readonly Regex rxBearer = new Regex(@"^bearer\s+(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex rxEndSlash = new Regex(@"\/$", RegexOptions.Compiled);

        /// <summary>
        /// 存放 RPCFunc
        /// </summary>
        private static readonly ConcurrentDictionary<string, IRPCFunc> funcDict = new ConcurrentDictionary<string, IRPCFunc>();

        /// <summary>
        /// 存到 RPCFunc 的 methodName, JRPCAuthorizedAttribute
        /// </summary>
        private static readonly ConcurrentDictionary<string, IEnumerable<JRPCAuthorizedAttribute>> authorizedDict = new ConcurrentDictionary<string, IEnumerable<JRPCAuthorizedAttribute>>();

        /// <summary>
        /// store serviceName, serviceId
        /// </summary>
        private static readonly Dictionary<string, string> servicesDict = new Dictionary<string, string>();
        private static readonly List<Type> registeredServiceType = new List<Type>();

        static IServiceDiscovery _serviceDiscovery;
        static IMembership _membership;
        static ITokenManager _tokenManager;

        public static void UseServiceDiscovery(IServiceDiscovery serviceDiscovery)
        {
            _serviceDiscovery = serviceDiscovery;
        }

        public static void UserTokenManager(ITokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        public static void UserMembership(IMembership membership)
        {
            _membership = membership;
        }

        /// <summary>
        /// 判斷是否有權限
        /// </summary>
        /// <param name="req"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool CheckAuthorized(RPCRequest req)
        {
            if (!authorizedDict.TryGetValue(req.RPCFuncName, out IEnumerable<JRPCAuthorizedAttribute> attrs))
                return true;

            if (_membership == null)
                throw new JRPCException("The IMembership is null, cannot perform auhorization check");

            if (_tokenManager == null)
                throw new JRPCException("The ITokenManager is null, cannot perform auhorization check");

            var account = _tokenManager.ValidateToken(req.AccessToken);

            if (string.IsNullOrWhiteSpace(account))
                return false;

            req.Account = account;

            if (_membership.GetUserByAccount(account).IsAdmin)
                return true;

            foreach (var attr in attrs)
            {
                if (attr.Resource == null && attr.Actions == null)
                {
                    // 沒有標注任何 resource, 表示只要登入就可以呼叫
                    return true;
                }

                if (attr.Actions.Count() == 0)
                {
                    if (_membership.IsGranted(account, attr.Resource))
                        return true;
                }

                foreach (var action in attr.Actions)
                {
                    if (_membership.IsGranted(account, attr.Resource, action))
                        return true;
                }
            }

            return true;
        }

        /// <summary>
        /// 新增service物件，將service內含有[RPCFunc]的方法都註冊為Microservice
        /// </summary>
        /// <param name="serviceObj"></param>
        public static void AddService(object serviceObj)
        {
            RegisterFuncs(serviceObj);
        }

        static bool ShuttingDown;
        static void Shutdown()
        {
            if (_serviceDiscovery == null)
                return;

            if (!ShuttingDown)
            {
                ShuttingDown = true;

                foreach (var kv in servicesDict)
                    _serviceDiscovery.DeregisterService(kv.Value);
            }
        }

        public static Task<RPCResponse> ExecuteAsync(RPCRequest input)
        {
            if (funcDict.TryGetValue(input.RPCFuncName, out IRPCFunc f))
            {
                return f.ExecuteAsync(input);
            }
            else
            {
                throw new JRPCMethodNotFoundException($"cannot find method: {input.RPCFuncName}");
            }
        }

        /// <summary>
        ///  註冊Service類別，含有[JRPCMethod]的方法
        /// </summary>
        /// <param name="svcObj"></param>
        /// <param name="withConsul">是否註冊到consul</param>
        public static void RegisterFuncs(object svcObj)
        {
            foreach (var m in svcObj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                // 不處理沒標註[RPCFunc]的方法
                var jmethod = (JRPCMethodAttribute)m.GetCustomAttribute(typeof(JRPCMethodAttribute), false);
                if (jmethod == null)
                    continue;

                if (string.IsNullOrWhiteSpace(jmethod.Name))
                    throw new Exception("JMethod.Name cannot not empty");

                //取得該方法的參數
                var parameters = m.GetParameters();

                // 處理 JRPCAuthorizedAttribute
                var authorizedAttributes = m.GetCustomAttributes(typeof(JRPCAuthorizedAttribute), false) as IEnumerable<JRPCAuthorizedAttribute>;

                ProcessAuthorizedAttribute(authorizedAttributes, jmethod.Name);

                if (parameters.Length == 0 && m.ReturnType != typeof(void))
                {
                    AddNoParameterRPCFunc(m, svcObj, jmethod.Name);
                }
                else if (parameters.Length == 1 && m.ReturnType != typeof(void))
                {
                    AddOneParameterRPCFunc(m, svcObj, jmethod.Name);
                }
                else if (parameters.Length == 2 && m.ReturnType != typeof(void))
                {
                    AddTwoParameterRPCFunc(m, svcObj, jmethod.Name);
                }
                else
                {
                    throw new Exception($"{jmethod.Name}'s format does not match");
                }
            }
        }

        static void AddNoParameterRPCFunc(MethodInfo m, object svcObj, string methodName)
        {
            if (funcDict.ContainsKey(methodName))
                throw new Exception($"{methodName} duplicate register");

            var funcType = typeof(Func<>).MakeGenericType(m.ReturnType);
            var del = Delegate.CreateDelegate(funcType, svcObj, m);

            if (m.ReturnType.IsGenericType)
            { // e.g. Task<string> Func00_1()

                if (!m.ReturnType.Name.StartsWith("Task") || m.ReturnType.GenericTypeArguments?.Length != 1)
                    throw new Exception("mehtod must return a Task or Task<>");

                var f = (IRPCFunc)Activator.CreateInstance(
                    typeof(RPCFuncNoInputReply<>).MakeGenericType(m.ReturnType.GenericTypeArguments[0]),
                    del);

                funcDict.TryAdd(methodName, f);
            }
            else
            { // e.g. Task Func00_0()

                if (!m.ReturnType.Name.StartsWith("Task"))
                    throw new Exception("mehtod must return a Task");

                var f = (IRPCFunc)Activator.CreateInstance(
                    typeof(RPCFuncNoInputNoReply),
                    del);

                funcDict.TryAdd(methodName, f);
            }
        }

        static void AddOneParameterRPCFunc(MethodInfo m, object svcObj, string methodName)
        {
            if (funcDict.ContainsKey(methodName))
                throw new Exception($"{methodName} duplicate register");

            var inputType = m.GetParameters()[0].ParameterType;

            var funcType = typeof(Func<,>).MakeGenericType(inputType, m.ReturnType);
            var del = Delegate.CreateDelegate(funcType, svcObj, m);

            if (m.ReturnType.IsGenericType)
            {
                if (!m.ReturnType.Name.StartsWith("Task") || m.ReturnType.GenericTypeArguments?.Length != 1)
                    throw new Exception("mehtod must return a Task or Task<>");

                if (inputType == typeof(RPCRequest))
                { // e.g. Task<string> Func01_1(RPCRequest req)

                    var f = (IRPCFunc)Activator.CreateInstance(
                        typeof(RPCFuncNoInputReply<>).MakeGenericType(m.ReturnType.GenericTypeArguments[0]),
                        del);

                    funcDict.TryAdd(methodName, f);
                }
                else
                { // e.g. Task<string> Func10_1(string input)

                    var f = (IRPCFunc)Activator.CreateInstance(
                        typeof(RPCFuncInputReply<,>).MakeGenericType(inputType, m.ReturnType.GenericTypeArguments[0]),
                        del);

                    funcDict.TryAdd(methodName, f);
                }
            }
            else
            {
                if (!m.ReturnType.Name.StartsWith("Task"))
                    throw new Exception("mehtod must return a Task");

                if (inputType == typeof(RPCRequest))
                { // e.g. Task Func01_0(RPCRequest req)

                    var f = (IRPCFunc)Activator.CreateInstance(
                        typeof(RPCFuncNoInputNoReply),
                        del);

                    funcDict.TryAdd(methodName, f);
                }
                else
                { // e.g. Task Func10_0(string input)

                    var f = (IRPCFunc)Activator.CreateInstance(
                        typeof(RPCFuncInputNoReply<>).MakeGenericType(inputType),
                        del);

                    funcDict.TryAdd(methodName, f);
                }
            }
        }

        static void AddTwoParameterRPCFunc(MethodInfo m, object svcObj, string methodName)
        {
            if (funcDict.ContainsKey(methodName))
                throw new Exception($"{methodName} duplicate register");

            var inputType = m.GetParameters()[0].ParameterType;
            var rpcRequestType = m.GetParameters()[1].ParameterType;

            if (rpcRequestType != typeof(RPCRequest))
                throw new Exception($"{methodName}'s second parameter must be a RPCRequest");

            var funcType = typeof(Func<,,>).MakeGenericType(inputType, rpcRequestType, m.ReturnType);
            var del = Delegate.CreateDelegate(funcType, svcObj, m);

            if (m.ReturnType.IsGenericType)
            { // e.g. Task<string> Func11_1(string input, RPCRequest req)

                if (!m.ReturnType.Name.StartsWith("Task") || m.ReturnType.GenericTypeArguments?.Length != 1)
                    throw new Exception("mehtod must return a Task or Task<>");

                var f = (IRPCFunc)Activator.CreateInstance(
                    typeof(RPCFuncInputReply<,>).MakeGenericType(inputType, m.ReturnType.GenericTypeArguments[0]),
                    del);

                funcDict.TryAdd(methodName, f);
            }
            else
            { // e.g. Task Func11_0(string input, RPCRequest req)

                if (!m.ReturnType.Name.StartsWith("Task"))
                    throw new Exception("mehtod must return a Task");

                var f = (IRPCFunc)Activator.CreateInstance(
                    typeof(RPCFuncInputNoReply<>).MakeGenericType(inputType),
                    del);

                funcDict.TryAdd(methodName, f);
            }
        }

        static void ProcessAuthorizedAttribute(IEnumerable<JRPCAuthorizedAttribute> attrs, string methodName)
        {
            if (attrs == null || attrs.Count() == 0)
                return;

            authorizedDict.TryAdd(methodName, attrs);
        }

        static void RegisterServiceDiscovery()
        {
            if (_serviceDiscovery != null)
            {
                foreach (var kv in servicesDict)
                    _serviceDiscovery.RegisterService(kv.Key, kv.Value, 0 /*PORT*/);
            }
        }
    }
}
