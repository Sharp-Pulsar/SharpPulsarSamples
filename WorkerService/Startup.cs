using Echo.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using WorkerService.Hubs;

namespace WorkerService
{
    #region Startup
    public class Startup
    {

        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            
            var configuration = Configuration;
            PulsarSettings options = configuration.GetSection("Pulsar").Get<PulsarSettings>();
            services.AddSingleton(options);
            services.AddSignalR((o) => {
                o.EnableDetailedErrors = true;
            });
            services.AddHostedService<Worker>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCorsMiddleware();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<EchoHub>("/hubs/echo");
            });
        }
    }
    #endregion
}