using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace AspNetCore.ScopeValidation.Test
{
    public class WildcardScopeTests
    {





        private const string ScopeClaimType = "scope";


        private readonly HttpClient _client;


        public WildcardScopeTests()
        {

            var identity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ScopeClaimType, "wildcard_scope")
            });


            var startup = new TestServerStartup(o =>
            {
                o.AnonymousRoutes = new List<string> { "/anonymous/route" };
                o.AuthenticationScheme = "test_scheme";
                o.ScopeClaimType = "scope";
                o.ScopeSchemes = new List<ScopeScheme>
                {
                    new ScopeScheme
                    {
                        WildcardScope = "wildcard_scope",
                        AllowedScopes = new List<Scope>
                        {
                            new Scope
                            {
                                RequestMethod = HttpMethod.Post,
                                AllowedScopes = new List<string>{"valid_post_scope"}
                            },
                            new Scope
                            {
                                RequestMethod = HttpMethod.Get,
                                AllowedScopes = new List<string>{"valid_get_scope"}
                            }
                        }
                    }
                };
            }, new ClaimsPrincipal(identity));


            var server = new TestServer(new WebHostBuilder()
                .ConfigureServices(startup.ConfigureServices)
                .Configure(startup.Configure));

            _client = server.CreateClient();
        }




        [Fact]
        public async Task TestValidPost()
        {
            // ACT
            var response = await _client.PostAsync("/restricted/route", new StringContent("response body"));


            // ASSERT
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }



        [Fact]
        public async Task TestValidGet()
        {
            // ACT
            var response = await _client.GetAsync("/restricted/route");


            // ASSERT
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }



    }
}
