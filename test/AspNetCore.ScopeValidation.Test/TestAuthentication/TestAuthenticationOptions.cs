using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AspNetCore.ScopeValidation.Test.TestAuthentication
{
    public class TestAuthenticationOptions: AuthenticationSchemeOptions
    {

        public ClaimsPrincipal Principal;

    }
}
