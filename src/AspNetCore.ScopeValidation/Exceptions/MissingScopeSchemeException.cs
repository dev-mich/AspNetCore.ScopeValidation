using System;


namespace AspNetCore.ScopeValidation.Exceptions
{
    public class MissingScopeSchemeException: Exception
    {

        public MissingScopeSchemeException(string path): base($"Scope scheme not found for non anonymous route {path}") { }

    }
}
