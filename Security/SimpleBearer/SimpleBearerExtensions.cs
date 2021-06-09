using AcmeChallengeResponder.Security.SimpleBearer;
using Microsoft.AspNetCore.Authentication;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SimpleBearerExtensions
    {
        public static AuthenticationBuilder AddSimpleBearer(this AuthenticationBuilder builder)
        {
            return AddSimpleBearer(builder, null);
        }

        public static AuthenticationBuilder AddSimpleBearer(this AuthenticationBuilder builder, Action<SimpleBearerOptions>? configureOptions)
        {
            return builder.AddScheme<SimpleBearerOptions, SimpleBearerHandler>(SimpleBearerDefaults.AuthenticationScheme, configureOptions);
        }
    }
}
