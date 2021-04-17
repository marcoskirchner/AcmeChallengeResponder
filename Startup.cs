using AcmeChallengeRestResponder.Security.SimpleBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AcmeChallengeRestResponder
{
    public class Startup
    {
        const string PATH_PATTERN = "{**path}";
        private readonly Dictionary<string, string> _challengeTokens = new();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(SimpleBearerDefaults.AuthenticationScheme)
                .AddSimpleBearer(options => Configuration.Bind("SimpleBearer", options));
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPut(PATH_PATTERN, async context =>
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
                });
                endpoints.MapGet(PATH_PATTERN, async context =>
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
                }).AllowAnonymous();
                endpoints.MapDelete(PATH_PATTERN, context =>
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
                });
            });
        }
    }
}
