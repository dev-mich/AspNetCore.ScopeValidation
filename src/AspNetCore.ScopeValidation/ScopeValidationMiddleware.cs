using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.ScopeValidation.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.ScopeValidation
{
    public class ScopeValidationMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ScopeValidationOptions _options;
        private readonly ILogger<ScopeValidationMiddleware> _logger;

        public ScopeValidationMiddleware(RequestDelegate next, IOptions<ScopeValidationOptions> options, ILogger<ScopeValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }


        public async Task Invoke(HttpContext context)
        {

            _logger.LogDebug($"Begin scope validation for action: {context.Request.Path}");

            // get request method and path
            var requestMethod = context.Request.Method;
            var requestPath = context.Request.Path;


            // ensure that path is not not anonymous
            if (_options.AnonymousRoutes != null && _options.AnonymousRoutes.Any(r => requestPath.Value.StartsWith(r, StringComparison.InvariantCultureIgnoreCase)))
            {
                _logger.LogDebug("Validation skipped cause route allow anonymous users");

                await _next(context);

                return;
            }



            // ensure user is authenticated cause route is not anonymous

            AuthenticateResult result = await context.AuthenticateAsync(_options.AuthenticationScheme);

            if (!result.Succeeded)
            {
                _logger.LogWarning("User is anonymous, return 401");
                Unauthorized(context);
                return;
            }



            if (!Validate(requestPath, requestMethod, result.Principal))
            {
                _logger.LogWarning("Scope validation failed. Return 403");
                await Forbidden(context);
                return;
            }



            // validation is ok, go on
            _logger.LogDebug("Scope validation success");

            await _next(context);



        }





        private void ValidateConfiguration()
        {
            if (_options.ScopeSchemes == null)

                throw new ArgumentNullException(nameof(_options.ScopeSchemes));


            foreach (var optionsScopeScheme in _options.ScopeSchemes)
            {
                if (optionsScopeScheme.AllowedScopes == null)

                    throw new ArgumentNullException(nameof(optionsScopeScheme.AllowedScopes));

                if (!optionsScopeScheme.AllowedScopes.Any())

                    throw new InvalidOperationException(nameof(optionsScopeScheme.AllowedScopes));

            }

        }






        private bool Validate(string path, string method, ClaimsPrincipal principal)
        {
            // ensure that configuration is valid
            ValidateConfiguration();


            var scopeClaims = principal.FindAll(_options.ScopeClaimType);
            _logger.LogInformation("Scopes found on principal: {scopes}", scopeClaims);

            if (scopeClaims == null || !scopeClaims.Any())

                return false;

            var grantedScopes = scopeClaims.Select(s => s.Value);


            // search for a valid scheme
            var scopeScheme = _options.ScopeSchemes.FirstOrDefault(s => path.StartsWith(s.PathTemplate, StringComparison.InvariantCultureIgnoreCase));

            if (scopeScheme == null)
            {

                // a scope scheme for the given request was not found, check if a generic scheme exist (empty PathTemplate)
                scopeScheme = _options.ScopeSchemes.FirstOrDefault(s => string.IsNullOrEmpty(s.PathTemplate));

                if(scopeScheme == null)
                    throw new MissingScopeSchemeException(path);

            }

            // check if wildcard scope is granted
            if (grantedScopes.Any(s => s == scopeScheme.WildcardScope))
                return true;


            // wildcard scope is not granted, search for specific scope
            var allowedScopes = scopeScheme.AllowedScopes.Where(s =>s.RequestMethod.Method.Equals(method)).SelectMany(s => s.AllowedScopes);

            var intersection = allowedScopes.Intersect(grantedScopes);

            return intersection.Any();
        }



        public void Unauthorized(HttpContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
        }



        public async Task Forbidden(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync($"invalid scopes");
        }
    }
}
