
using Echo.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Pulsar
{
    public class HostService : IHostedService
    {
        public static PulsarConnector PulsarConnector;
        public HostService(PulsarSettings pulsarSettings, IWebHostEnvironment env)
        {
            PulsarConnector = new PulsarConnector(pulsarSettings , env);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            PulsarConnector.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            PulsarConnector.Stop();
            return Task.CompletedTask;
        }
    }

}
