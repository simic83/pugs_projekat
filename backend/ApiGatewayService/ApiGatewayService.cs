using System.Fabric;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiGatewayService.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ApiGatewayService
{
    internal sealed class ApiGatewayService : StatelessService
    {
        private const string FrontendCorsPolicy = "FrontendCorsPolicy";

        public ApiGatewayService(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, "Starting Kestrel on {0}", url);
                        var settings = FabricConfigurationProvider.Load(serviceContext);
                        settings.EnsureJwtConfigured();

                        return Host.CreateDefaultBuilder()
                            .ConfigureWebHostDefaults(webBuilder =>
                            {
                                webBuilder
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .ConfigureServices(services =>
                                    {
                                        services.AddSingleton(serviceContext);
                                        services.AddSingleton(settings);
                                        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                                            .AddJwtBearer(options =>
                                            {
                                                options.TokenValidationParameters = new TokenValidationParameters
                                                {
                                                    ValidateIssuerSigningKey = true,
                                                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtSecret)),
                                                    ValidateIssuer = true,
                                                    ValidIssuer = settings.JwtIssuer,
                                                    ValidateAudience = true,
                                                    ValidAudience = settings.JwtAudience,
                                                    ValidateLifetime = true,
                                                    ClockSkew = TimeSpan.Zero,
                                                    NameClaimType = JwtRegisteredClaimNames.Sub,
                                                    RoleClaimType = ClaimTypes.Role
                                                };
                                            });
                                        services.AddAuthorization();
                                        if (settings.CorsAllowedOrigins.Count > 0)
                                        {
                                            services.AddCors(options =>
                                            {
                                                options.AddPolicy(FrontendCorsPolicy, policy =>
                                                {
                                                    policy
                                                        .WithOrigins(settings.CorsAllowedOrigins.ToArray())
                                                        .AllowAnyHeader()
                                                        .AllowAnyMethod();
                                                });
                                            });
                                        }

                                        services.AddControllers();
                                    })
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Configure(app =>
                                    {
                                        app.UseRouting();
                                        if (settings.CorsAllowedOrigins.Count > 0)
                                        {
                                            app.UseCors(FrontendCorsPolicy);
                                        }

                                        app.UseAuthentication();
                                        app.UseAuthorization();
                                        app.UseEndpoints(endpoints => endpoints.MapControllers());
                                    });
                            })
                            .Build();
                    }))
            };
        }
    }
}
