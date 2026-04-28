using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TravelPlanner.ApiGatewayService;

internal sealed class ApiGatewayService : StatelessService
{
    public ApiGatewayService(StatelessServiceContext context)
        : base(context)
    {
    }

    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
    {
        return new[]
        {
            new ServiceInstanceListener(serviceContext =>
                new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    Host.CreateDefaultBuilder()
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder
                                .UseKestrel()
                                .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                .UseUrls(url)
                                .ConfigureServices(services =>
                                {
                                    services.AddSingleton(serviceContext);
                                    services.AddControllers();
                                })
                                .Configure(app =>
                                {
                                    app.UseRouting();
                                    app.UseAuthorization();
                                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                                });
                        })
                        .Build()))
        };
    }
}
