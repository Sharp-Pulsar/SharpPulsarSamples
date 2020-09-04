
using Akka.Actor;
using Echo.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;
using Pulsar;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Handlers;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading.Tasks;

//https://appdevmusings.com/asp-net-core-2-1-web-api-load-app-configuration-from-appsettings-json-dockerfile-environment-variables-azure-key-vault-secrets-and-kubernetes-configmaps-secrets
//https://medium.com/@fbeltrao/automatically-reload-configuration-changes-based-on-kubernetes-config-maps-in-a-net-d956f8c8399a
//https://appdevmusings.com/azure-kubernetes-service-aks-deploying-angular-asp-net-core-and-sql-server-on-linux
namespace Web.Controllers
{
    [Route("api")]
    [ApiController]
    public class PulsarController : ControllerBase
    {
        public PulsarController()
        {
        }
        [Route("submit")]
        [HttpPost]
        public async Task<string> Submit([FromBody] Dictionary<string, string> payload)
        {
            try
            {
                var id = payload["Id"];
                var text = payload["Text"];
                var echo = new Echo.Common.Echo()
                {
                    Id = id,
                    Text = text
                };
                var metadata = new Dictionary<string, object>
                {
                    ["Framework"] = "Aspnet Core"
                };
                var send = new Send(echo, metadata.ToImmutableDictionary());
                var receipt = await Task.Factory.Run(() => {
                    return HostService.PulsarConnector.Send(send);
                });
                return JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception e)
            {
                return $"Failed with message:'{e.Message}'";

            }
        }
    }
}
