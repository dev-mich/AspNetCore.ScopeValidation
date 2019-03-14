using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AspNetCore.ScopeValidation.Test.Infrastructure
{
    public class TestAuthenticationOptions: AuthenticationSchemeOptions
    {

        public ClaimsPrincipal Principal;

    }
}
