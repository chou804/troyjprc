using DnsClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TroyLab.JRPC.Client
{
    public class JClientOptions
    {
        public bool UseHTTPS { get; set; }
    }
    public class JClient
    {
        static ConcurrentDictionary<Uri, HttpClient> clientDict = new ConcurrentDictionary<Uri, HttpClient>();
        static TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(30);
        static JClientOptions ClientOptions = new JClientOptions();

        static JClient()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        public static void Setup(Action<JClientOptions> options)
        {
            options?.Invoke(ClientOptions);
        }

        /// <summary>
        /// call no return value (only Task) JRPCMethod
        /// </summary>
        /// <param name="method"></param>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        /// <param name="serviceUri"></param>
        /// <returns></returns>
        public static async Task<JRPCError> CallAsync(string method, object request, TimeSpan? timeout = null, Uri serviceUri = null)
        {
            var (e, v) = await CallAsync<object>(method, request, timeout, serviceUri);
            return e;
        }
        public static async Task<(JRPCError err, TReply reply)> CallAsync<TReply>(string method, object request, TimeSpan? timeout = null, Uri serviceUri = null)
        {
            if (timeout == null)
                timeout = DEFAULT_TIMEOUT;

            try
            {
                method = method.ToLower();

                if (serviceUri == null)
                {
                    var serviceEndPoint = await GetServiceAddressAsync(method);
                    Ensure.ThrowIf(string.IsNullOrWhiteSpace(serviceEndPoint), $"service is not available temporarily: {method}");
                    serviceUri = ClientOptions.UseHTTPS ? new Uri($"https://{serviceEndPoint}") : new Uri($"http://{serviceEndPoint}");
                }

                var jreq = new JRPCRequest
                {
                    Params = new JRaw(RPCPacker.Pack(request)),
                    Method = method
                };

                if (!clientDict.TryGetValue(serviceUri, out HttpClient client))
                {
                    client = new HttpClient
                    {
                        BaseAddress = serviceUri
                    };

                    clientDict.TryAdd(serviceUri, client);
                }

                using (var content = new StringContent(RPCPacker.Pack(jreq), Encoding.UTF8, "application/json"))
                {
                    var response = await client.PostAsync("jrpc", content);
                    response.EnsureSuccessStatusCode();

                    var s = await response.Content.ReadAsStringAsync();
                    var v = RPCPacker.Unpack<JRPCResponse>(s);
                    var r = RPCPacker.Unpack<TReply>(v.Result?.ToString());

                    return (v.Error, r);
                }
            }
            catch (Exception ex)
            {
                return (new JRPCError { Message = "Error", Data = ex.Message }, default(TReply));
            }
        }

        public static async Task<JRPCResponse> ForwardAsync(JRPCRequest req, TimeSpan? timeout = null, Uri serviceUri = null)
        {
            if (serviceUri == null)
            {
                var serviceEndPoint = await GetServiceAddressAsync(req.Method);
                Ensure.ThrowIf(string.IsNullOrWhiteSpace(serviceEndPoint), $"service is not available temporarily: {req.Method}");
                serviceUri = ClientOptions.UseHTTPS ? new Uri($"https://{serviceEndPoint}") : new Uri($"http://{serviceEndPoint}");
            }

            if (!clientDict.TryGetValue(serviceUri, out HttpClient client))
            {
                client = new HttpClient
                {
                    BaseAddress = serviceUri,
                };

                if (timeout != null)
                {
                    client.Timeout = timeout.Value;
                }

                clientDict.TryAdd(serviceUri, client);
            }

            using (var content = new StringContent(RPCPacker.Pack(req), Encoding.UTF8, "application/json"))
            {
                var response = await client.PostAsync("jrpc", content);
                response.EnsureSuccessStatusCode();

                var s = await response.Content.ReadAsStringAsync();
                var v = RPCPacker.Unpack<JRPCResponse>(s);
                return v;
            }
        }

        static async Task<string> GetServiceAddressAsync(string fnName)
        {
            var serviceName = ServiceNameHelper.ComposeServiceName(fnName);
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8600);
            var dnsclient = new LookupClient(endpoint) { UseCache = true, Timeout = TimeSpan.FromSeconds(5) };

            var serviceAddr = "";

            try
            {
                // query consul dns service
                var dnsresult = await dnsclient.ResolveServiceAsync("service.consul", serviceName).ConfigureAwait(false);
                if (dnsresult.Count() == 0)
                {
                    return serviceAddr;
                }

                var addr = dnsresult.First().AddressList.FirstOrDefault();
                var port = dnsresult.First().Port;
                serviceAddr = $"{addr}:{port}";
            }
            catch (DnsResponseException ex)
            {
                throw new Exception("cannot connection to consul", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"cannot get service address, serviceName:{serviceName}", ex);
            }

            return serviceAddr;
        }
    }
}
