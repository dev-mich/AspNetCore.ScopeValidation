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

    public class GenericScopeSchemeTests
    {


        private const string ScopeClaimType = "scope";


        private readonly HttpClient _client;


        public GenericScopeSchemeTests()
        {

            var identity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ScopeClaimType, "valid_post_scope"),
                new Claim(ScopeClaimType, "invalid_get_scope")
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
            var response1 = await _client.PostAsync("/restricted/route", new StringContent("response body"));
            var response2 = await _client.PostAsync("/restricted/route2", new StringContent("response body"));


            // ASSERT
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        }





        [Fact]
        public async Task TestInvalidGet()
        {
            // ACT
            var response1 = await _client.GetAsync("/restricted/route");
            var response2 = await _client.GetAsync("/restricted/route");


            var responseContent1 = await response1.Content.ReadAsStringAsync();
            var responseContent2 = await response2.Content.ReadAsStringAsync();



            // ASSERT
            Assert.Equal(HttpStatusCode.Forbidden, response1.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, response2.StatusCode);

            Assert.Equal("invalid scopes", responseContent1);
            Assert.Equal("invalid scopes", responseContent2);

        }



    }
}
