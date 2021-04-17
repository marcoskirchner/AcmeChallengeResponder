using AcmeChallengeRestResponder.Security.SimpleBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AcmeChallengeRestResponder
{
    public class Startup
    {
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
                const string PATH_PATTERN = "{**path}";
                
                endpoints.MapGet(PATH_PATTERN, EndpointHandlers.HandleGet).AllowAnonymous(); /* the GET endpoint MUST allow anonymous access */
                endpoints.MapPut(PATH_PATTERN, EndpointHandlers.HandlePut);
                endpoints.MapDelete(PATH_PATTERN, EndpointHandlers.HandleDelete);
            });
        }
    }
}
