using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TroyLab.JRPC;
using TroyLab.JRPC.AspNetCore;
using DemoApp.Services;
using DemoApp.Data;

namespace DemoApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // JRPC
            services.AddJPRC(opt =>
            {
                opt.UseAuthentication<FakeMembershipStore, TokenKeyStore>();
            });

            services.AddJPRCService<EchoService>();
            services.AddJPRCService<AuthService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // JRPC
            app.UseJRPC();
        }
    }
}
