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
    interface IRPCFunc { Task<RPCResponse> ExecuteAsync(RPCRequest request); }

    public class RPCServer
    {
        static readonly Regex rxBearer = new Regex(@"^bearer\s+(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex rxEndSlash = new Regex(@"\/$", RegexOptions.Compiled);
        static IServiceDiscovery _serviceDiscovery;

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
            var isAuthorized = false;

            if (authorizedDict.TryGetValue(req.RPCFuncName, out IEnumerable<JRPCAuthorizedAttribute> attrs))
            {
                if (_membership == null)
                    throw new JRPCException("The IMembership is null, cannot perform auhorization check");

                if (_tokenManager == null)
                    throw new JRPCException("The ITokenManager is null, cannot perform auhorization check");

                var account = _tokenManager.ValidateToken(req.AccessToken);

                if (string.IsNullOrWhiteSpace(account))
                    return false;

                req.Account = account;
                if (_membership.GetUserByAccount(account).IsAdmin)
                {
                    // admin 有所有權限
                    return true;
                }

                foreach (var attr in attrs)
                {
                    if (attr.Resource == null && attr.Actions == null)
                    {
                        // 沒有標注任何 resource, 表示只要登入就可以呼叫
                        return true;
                    }

                    if (attr.Resource == "ROOT")
                        return true;

                    if (attr.Actions.Count() == 0)
                    {
                        isAuthorized = _membership.IsGranted(account, attr.Resource);

                        if (isAuthorized)
                        {
                            return true;
                        }
                    }

                    foreach (var action in attr.Actions)
                    {
                        isAuthorized = _membership.IsGranted(account, attr.Resource, action);

                        if (isAuthorized)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                // 目前預設, 沒有標 Authorized　的是允許匿名
                isAuthorized = true;
            }

            return isAuthorized;
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

                Console.WriteLine("deregistered all services");
                //Environment.Exit(exitCode);
                return;
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

        static void Add<TRequest, TResponse>(Func<TRequest, Task<TResponse>> func)
        {
            try
            {
                var inputType = typeof(TRequest);
                var methodName = ServiceNameHelper.ComposeFuncName(inputType);

                if (funcDict.ContainsKey(methodName))
                    throw new Exception($"{methodName} 重覆註冊");

                funcDict.TryAdd(methodName, new GeneralRPCFunc<TRequest, TResponse>(new Func<TRequest, Task<TResponse>>(func)));
            }
            catch (Exception ex)
            {
                throw new Exception("註冊RPC Func時異常", ex);
            }
        }

        /// <summary>
        /// 註冊二個參數包含 RPCRequest, 一個回傳Task`1的方法
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="func"></param>
        static void AddWithRequest<TRequest, TResponse>(Func<TRequest, RPCRequest, Task<TResponse>> func)
        {
            try
            {
                var inputType = typeof(TRequest);
                var methodName = ServiceNameHelper.ComposeFuncName(inputType);

                if (funcDict.ContainsKey(methodName))
                    throw new Exception($"{methodName} 重覆註冊");

                funcDict.TryAdd(methodName, new GeneralRPCFunc<TRequest, TResponse>(new Func<TRequest, RPCRequest, Task<TResponse>>(func)));
            }
            catch (Exception ex)
            {
                throw new Exception("註冊RPC Func時異常", ex);
            }
        }

        /// <summary>
        ///  註冊Service類別，含有[JFunc]的方法
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

                //取得該方法的參數
                var parameters = m.GetParameters();

                if (parameters.Length == 0)
                    throw new Exception("所有RPCFunc都必須要有一個以上的參數");

                var inputType = parameters[0].ParameterType; //Eg HelloInput

                // 處理 JRPCAuthorizedAttribute
                var authorizedAttributes = m.GetCustomAttributes(typeof(JRPCAuthorizedAttribute), false) as IEnumerable<JRPCAuthorizedAttribute>;
                //var authorizedAttribute = m.GetCustomAttribute(typeof(JRPCAuthorizedAttribute), false) as JRPCAuthorizedAttribute;

                ProcessAuthorizedAttribute(authorizedAttributes, inputType);

                if (parameters.Length == 1 && m.ReturnType != typeof(void) && m.ReturnType != typeof(Task))
                {//一個參數及一個回傳值的版本, Eg Task<HelloView> Hello(HelloInput input)

                    var viewType = m.ReturnType; //Eg Task<HelloView>

                    //建立方法的 Delegate
                    var funcType = typeof(Func<,>).MakeGenericType(inputType, viewType);
                    var del = Delegate.CreateDelegate(funcType, svcObj, m);

                    //執行 Add<TRequest, TResponse>(Func<TRequest, Task<TResponse>> func)
                    if (viewType.IsGenericType)
                    {
                        if (!viewType.Name.StartsWith("Task") || viewType.GenericTypeArguments?.Length != 1)
                            throw new Exception("方法的回傳值必需是Task or Task<>");

                        var addMethod = typeof(RPCServer).GetMethod(nameof(Add), BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(inputType, viewType.GenericTypeArguments[0]);
                        addMethod.Invoke(null, new object[] { del });
                    }
                }
                else if (parameters.Length == 2 && m.ReturnType != typeof(void) && m.ReturnType != typeof(Task))
                {//兩個參數及一個回傳值的版本, Eg Task<HelloView> Hello(HelloInput input, RPCOptions opt)

                    var requestType = parameters[1].ParameterType; // Eg RPCOptions
                    var viewType = m.ReturnType; //Eg Task<HelloView>

                    if (requestType != typeof(RPCRequest)) throw new Exception("第二個參數必須是 RPCRequest 型別");

                    //建立方法的 Delegate
                    var funcType = typeof(Func<,,>).MakeGenericType(inputType, requestType, viewType);
                    var del = Delegate.CreateDelegate(funcType, svcObj, m);

                    //執行 AddWithRPCOptions<TRequest, TResponse>(Func<TRequest, RPCRequest, Task<TResponse>> func)
                    if (viewType.IsGenericType)
                    {
                        if (!viewType.Name.StartsWith("Task") || viewType.GenericTypeArguments?.Length != 1)
                            throw new Exception("方法的回傳值必需是Task or Task<>");

                        var addMethod = typeof(RPCServer).GetMethod(nameof(AddWithRequest), BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(inputType, viewType.GenericTypeArguments[0]);
                        addMethod.Invoke(null, new object[] { del });
                    }
                }
                else
                {
                    throw new Exception("RPCFunc 的格式不符");
                }
            }
        }

        static void ProcessAuthorizedAttribute(IEnumerable<JRPCAuthorizedAttribute> attrs, Type inputType)
        {
            if (attrs == null || attrs.Count() == 0)
                return;

            var methodName = ServiceNameHelper.ComposeFuncName(inputType);
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

    /// <summary>
    /// 回傳Task`1
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    class GeneralRPCFunc<TInput, TReply> : IRPCFunc
    {
        readonly Func<TInput, Task<TReply>> func;
        readonly Func<TInput, RPCRequest, Task<TReply>> funcWithRPCRequest;

        public GeneralRPCFunc(Func<TInput, Task<TReply>> func)
        {
            this.func = func;
        }

        public GeneralRPCFunc(Func<TInput, RPCRequest, Task<TReply>> func)
        {
            this.funcWithRPCRequest = func;
        }

        public async Task<RPCResponse> ExecuteAsync(RPCRequest request)
        {
            var reply = new RPCResponse();

            if (funcWithRPCRequest != null)
            {
                reply.Pack(await funcWithRPCRequest.Invoke(request.Unpack<TInput>(), request));
            }
            else
            {
                reply.Pack(await func.Invoke(request.Unpack<TInput>()));
            }

            return reply;
        }
    }
}
