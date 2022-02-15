using CSG.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.Extensions
{
    public static class AppRepositories
    {
        public static IServiceCollection AddAppRepositories(this IServiceCollection services)
        {
            services.AddScoped<ProductRepo>();
            services.AddScoped<RequestRepo>();
            services.AddScoped<ServiceAndPriceRepo>();
            return services;
        }
    }
}
