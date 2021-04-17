using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AcmeChallengeRestResponder.Security.SimpleBearer
{
    public class SimpleBearerHandler : AuthenticationHandler<SimpleBearerOptions>
    {
        public SimpleBearerHandler(IOptionsMonitor<SimpleBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorization = (string)Request.Headers[HeaderNames.Authorization];
            if (string.IsNullOrEmpty(authorization))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string token = null;
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }


            if (token == Options.SecurityToken)
            {
                var identity = new ClaimsIdentity(Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var authTicket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(authTicket));
            }
            else
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
            }
        }
    }
}
