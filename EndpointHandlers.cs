using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AcmeChallengeRestResponder
{
    static class EndpointHandlers
    {
        private static readonly Dictionary<string, string> _challengeTokens = new();

        internal static async Task HandleGet(HttpContext context)
        {
            var currentUrl = context.Request.GetDisplayUrl();
            if (_challengeTokens.TryGetValue(currentUrl, out var token))
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync(token);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }

        internal static async Task HandlePut(HttpContext context)
        {
            var contentLength = context.Request.ContentLength;
            if (!contentLength.HasValue || contentLength.Value <= 0 || context.Request.ContentLength > 500)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            var currentUrl = context.Request.GetDisplayUrl();

            var buffer = new byte[(int)contentLength];
            var bytesRead = await context.Request.Body.ReadAsync(buffer, 0, (int)contentLength);
            var token = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            _challengeTokens[currentUrl] = token;
            context.Response.StatusCode = StatusCodes.Status201Created;
        }

        internal static Task HandleDelete(HttpContext context)
        {
            var currentUrl = context.Request.GetDisplayUrl();
            if (_challengeTokens.ContainsKey(currentUrl))
            {
                _challengeTokens.Remove(currentUrl);
                context.Response.StatusCode = StatusCodes.Status204NoContent;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }

            return Task.CompletedTask;
        }
    }
}
