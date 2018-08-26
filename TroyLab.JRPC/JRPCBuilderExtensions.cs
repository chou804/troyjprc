using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TroyLab.JRPC
{
    public static class JRPCBuilderExtensions
    {
        public static IServiceCollection AddJPRC(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }


            return services;
        }
    }
}
