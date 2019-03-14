using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.ScopeValidation.Extensions
{
    public static class DependencyInjection
    {


        public static void AddScopeValidation(this IServiceCollection services, Action<ScopeValidationOptions> options)
        {
            services.Configure(options);
        }




        public static IApplicationBuilder UseScopeValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ScopeValidationMiddleware>();
        }


    }
}
