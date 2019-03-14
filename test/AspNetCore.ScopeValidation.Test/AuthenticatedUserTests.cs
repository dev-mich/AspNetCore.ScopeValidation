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
    public class AuthenticatedUserTests
    {
        private const string ScopeClaimType = "scope";


        private readonly HttpClient _client;


        public AuthenticatedUserTests()
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
                o.ScopeSchemes = new List<Scope>
                {
                    new Scope
                    {
                        AllowedScopes = new List<string>{"valid_post_scope"},
                        PathTemplate = "/restricted/route",
                        RequestMethod = HttpMethod.Post
                    },
                    new Scope
                    {
                        AllowedScopes = new List<string>{"valid_get_scope"},
                        PathTemplate = "/restricted/route",
                        RequestMethod = HttpMethod.Get
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
        public async Task TestInvalidResponse()
        {
            // ACT
            var response = await _client.GetAsync("/restricted/route");

            var responseContent = await response.Content.ReadAsStringAsync();



            // ASSERT
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("invalid scopes", responseContent);

        }


    }
}
