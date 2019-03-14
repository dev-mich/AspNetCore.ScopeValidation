using System.Collections.Generic;
using System.Net.Http;

namespace AspNetCore.ScopeValidation
{
    public class ScopeValidationOptions
    {
        /// <summary>
        /// default "Bearer"
        /// </summary>
        public string AuthenticationScheme { get; set; }


        /// <summary>
        /// default "scope"
        /// </summary>
        public string ScopeClaimType { get; set; }


        /// <summary>
        /// List of scope validation rules
        /// </summary>
        public List<ScopeScheme> ScopeSchemes { get; set; }


        /// <summary>
        /// Routes that are not affected to scope validation, (api/v1 for example skip validation for all routes that starts with this template, pay attention)
        /// </summary>
        public List<string> AnonymousRoutes { get; set; }


        public ScopeValidationOptions()
        {
            AuthenticationScheme = "Bearer";
            ScopeClaimType = "scope";
        }


    }


    public class ScopeScheme
    {

        /// <summary>
        /// If set different paths can have different required scopes, if not set if for all paths that match request method and is not anonymous
        /// </summary>
        public string PathTemplate { get; set; }


        public List<Scope> AllowedScopes { get; set; }


        public ScopeScheme()
        {
            PathTemplate = string.Empty;
        }

    }



    public class Scope
    {
        /// <summary>
        /// Associated scope request method
        /// </summary>
        public HttpMethod RequestMethod { get; set; }


        /// <summary>
        /// List of valid scopes
        /// </summary>
        public IEnumerable<string> AllowedScopes { get; set; }
    }
}
