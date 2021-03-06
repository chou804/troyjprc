﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TroyLab.JRPC;
using TroyLab.JRPC.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public class JRPCOptions
    {
        IServiceCollection _services;
        public JRPCOptions(IServiceCollection services)
        {
            _services = services;
        }

        public void UseAuthentication<TMembershipRepo, TTokenKeyStorage>()
            where TMembershipRepo : IMembershipStore
            where TTokenKeyStorage : ITokenKeyStore
        {
            _services.AddScoped<IAuthentication, AuthenticationService>()
                     .AddScoped<ITokenManager, TokenManager>()
                     .AddScoped<IMembership, MembershipService>();

            _services.AddScoped(typeof(IMembershipStore), typeof(TMembershipRepo))
                     .AddScoped(typeof(ITokenKeyStore), typeof(TTokenKeyStorage));
        }
    }

    public static class JRPCBuilderExtensions
    {
        static JRPCOptions _options;

        static Regex rxLeadingSlashs = new Regex(@"^[\/\\]+", RegexOptions.Compiled);

        private static readonly List<Type> registeredServiceType = new List<Type>();

        public static IServiceCollection AddJPRC(this IServiceCollection services, Action<JRPCOptions> option)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            _options = new JRPCOptions(services);
            option(_options);

            return services;
        }

        public static IServiceCollection AddJPRCService<TJRPCService>(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped(typeof(TJRPCService));
            registeredServiceType.Add(typeof(TJRPCService));

            return services;
        }

        public static IApplicationBuilder UseJRPC(this IApplicationBuilder builder, string jrpcPath = "/jrpc")
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                foreach (var svcType in registeredServiceType)
                {
                    var svc = services.GetRequiredService(svcType);
                    RPCServer.AddService(svc);
                }

                RPCServer.UserTokenManager(services.GetService<ITokenManager>());
                RPCServer.UserMembership(services.GetService<IMembership>());
            }

            builder.Map(jrpcPath, JRPCHandler);

            return builder;
        }

        static void JRPCHandler(IApplicationBuilder app)
        {
            app.Run(async ctx =>
            {
                ctx.Response.ContentType = "application/json";

                var jres = new JRPCResponse();
                try
                {
                    string body;
                    using (var sr = new StreamReader(ctx.Request.Body, encoding: Encoding.UTF8))
                    {
                        body = sr.ReadToEnd();
                    }

                    //var rpcReq = new RPCRequest { Ctx = ctx };
                    var rpcReq = new RPCRequest
                    {
                        AccessToken = GetJWT(ctx.Request.Headers)
                    };

                    try
                    {
                        var jreq = RPCPacker.Unpack<JRPCRequest>(body);

                        // allow methodName path e.g. /jrpc/{methodName}
                        rpcReq.RPCFuncName = jreq.Method;
                        if (string.IsNullOrWhiteSpace(rpcReq.RPCFuncName) && ctx.Request.Path.HasValue)
                        {
                            rpcReq.RPCFuncName = rxLeadingSlashs.Replace(ctx.Request.Path.Value, "").ToLower();
                        }

                        // 允許 body 就直接是 PackedData e.g. { "Msg": "Hello" }
                        rpcReq.PackedData = jreq.Params != null ? jreq.Params.ToString() : body;

                        jres.Id = jreq.Id;
                    }
                    catch (Exception e)
                    {
                        jres.Error = new JRPCError(JRPCError.PARSE_ERROR, e.Message);
                        await ctx.Response.WriteAsync(RPCPacker.Pack(jres, true));
                    }

                    //var rpcResponse = await RPCServer.ExecuteAsync(rpcReq);
                    //jres.Result = new JRaw(rpcResponse.PackedData);

                    if (RPCServer.CheckAuthorized(rpcReq))
                    {
                        var rpcResponse = await RPCServer.ExecuteAsync(rpcReq);

                        jres.Result = new JRaw(rpcResponse.PackedData);
                    }
                    else
                    {
                        jres.Error = new JRPCError(401, "Permission denied", null);
                    }
                }
                catch (JRPCException ex)
                {
                    // 預期的錯誤
                    jres.Error = new JRPCError(1, ex.Message, null);
                    ctx.Response.StatusCode = ex.HttpStatusCode;
                }
                catch (JRPCMethodNotFoundException ex)
                {
                    jres.Error = new JRPCError(JRPCError.METHOD_NOT_FOUND, ex.Message);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    jres.Error = new JRPCError(JRPCError.INVALID_PARAMS, ex.Message);
                }
                catch (Exception ex)
                {
                    jres.Error = new JRPCError(JRPCError.INTERNAL_ERROR, ex.Message);
                }

                await ctx.Response.WriteAsync(RPCPacker.Pack(jres, true));
            });
        }

        static readonly Regex rxBearer = new Regex(@"^bearer\s+(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static string GetJWT(IHeaderDictionary headers)
        {
            if (headers.TryGetValue("Authorization", out StringValues jwtValue))
            {
                var m = rxBearer.Match(jwtValue.ToString());
                if (m.Success && m.Groups.Count == 2)
                {
                    return m.Groups[1].Value;
                }
            }
            return "";
        }
    }
}
