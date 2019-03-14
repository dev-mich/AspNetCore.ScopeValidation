using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace AspNetCore.ScopeValidation.Test.AnonymousUser
{
    public class AnonymousUserTests
    {
        private HttpClient _client;


        public AnonymousUserTests()
        {

            var startup = new TestServerStartup(o =>
            {
                o.AnonymousRoutes = new List<string>{"/anonymous/route"};
                o.AuthenticationScheme = "test_scheme";
                o.ScopeClaimType = "scope";
                o.ScopeSchemes = new List<ScopeScheme>
                {
                    new ScopeScheme
                    {
                        PathTemplate = "/restricted/route",
                        AllowedScopes = new List<Scope>
                        {
                            new Scope
                            {
                                RequestMethod = HttpMethod.Get,
                                AllowedScopes = new List<string>{"valid.scope"}
                            }
                        }
                    }
                };
            }, null);


            var server = new TestServer(new WebHostBuilder()
                .ConfigureServices(startup.ConfigureServices)
                .Configure(startup.Configure));

            _client = server.CreateClient();

        }



        [Fact]
        public async Task TestAnonymousRoute()
        {

            // ACT
            var response = await _client.GetAsync("/anonymous/route");


            // ASSERT
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }



        [Fact]
        public async Task TestAnonymousSubRoute()
        {

            // ACT
            var response = await _client.GetAsync("/anonymous/route/sub");


            // ASSERT
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }



        [Fact]
        public async Task TestRestrictedRoute()
        {
            // ACT
            var response = await _client.GetAsync("/restricted/route");


            // ASSERT
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }



        [Fact]
        public async Task TestRestrictedSubRoute()
        {
            // ACT
            var response = await _client.GetAsync("/restricted/route/sub");


            // ASSERT
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }


    }
}
