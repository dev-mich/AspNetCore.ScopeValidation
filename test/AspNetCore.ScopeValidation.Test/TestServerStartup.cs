using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.ScopeValidation.Extensions;
using AspNetCore.ScopeValidation.Test.TestAuthentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.ScopeValidation.Test
{
    public class TestServerStartup
    {

        private readonly Action<ScopeValidationOptions> _scopeOptions;
        private readonly ClaimsPrincipal _principal;

        public TestServerStartup(Action<ScopeValidationOptions> scopeOptions, ClaimsPrincipal principal)
        {

            _scopeOptions = scopeOptions;
            _principal = principal;

        }




        public void ConfigureServices(IServiceCollection collection)
        {
            collection.AddScopeValidation(_scopeOptions);


            collection.AddAuthentication().AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>(
                "test_scheme",
                o => { o.Principal = _principal; });

        }


        public void Configure(IApplicationBuilder app)
        {

            app.Use(async (ctx, next) =>
            {
                ctx.User = new ClaimsPrincipal();

                await next();
            });


            app.UseAuthentication();

            app.UseScopeValidation();


            app.Run(ctx =>
            {
                ctx.Response.StatusCode = 200;
                return Task.FromResult(0);
            });


        }


    }
}
