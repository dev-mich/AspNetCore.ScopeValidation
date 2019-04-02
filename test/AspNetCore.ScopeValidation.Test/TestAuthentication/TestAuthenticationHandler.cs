using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.ScopeValidation.Test.TestAuthentication
{
    public class TestAuthenticationHandler: AuthenticationHandler<TestAuthenticationOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Options.Principal == null)
                return Task.FromResult(AuthenticateResult.Fail("failed", new AuthenticationProperties()));

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(Options.Principal,
                new AuthenticationProperties(), "test_scheme")));
        }
    }
}
