
using Akka.Actor;
using Echo.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;
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

namespace Web.Controllers
{
    [Route("api")]
    [ApiController]
    public class PulsarController : ControllerBase
    {
        private readonly PulsarSystem _pulsarSystem;
        private IActorRef _producer;
        private IConfiguration _configuration; 
        private string _contentRoot;
        private AvroSchema _schema;
        private PulsarSettings _pulsarSettings;
        public PulsarController(PulsarSettings pulsarSettings, IWebHostEnvironment env)
        {
            _pulsarSettings = pulsarSettings;
            _contentRoot = env.ContentRootPath;
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl(pulsarSettings.ServiceUrl)
                .ConnectionsPerBroker(1)
                .UseProxy(pulsarSettings.UseProxy)
                .OperationTimeout(pulsarSettings.OperationTimeout)
                .AllowTlsInsecureConnection(false)
                .ProxyServiceUrl(pulsarSettings.ProxyServiceUrl, ProxyProtocol.SNI)
                .Authentication(new AuthenticationDisabled())
                .ClientConfigurationData;

            _pulsarSystem = PulsarSystem.GetInstance(clientConfig);
            _schema = AvroSchema.Of(typeof(Echo.Common.Echo));
            _producer = Producer();
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
                    return _pulsarSystem.Send(send, _producer);
                });
                return JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception e)
            {
                return $"Failed with message:'{e.Message}'";

            }
        }
        private IActorRef Producer()
        {
            var topic = _pulsarSettings.Topic;
            var producerListener = new DefaultProducerListener((o) =>
            {
                
            }, s =>
            {
                
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName($"Web-{topic}-{Guid.NewGuid()}")
                .Topic(topic)
                .Schema(_schema)
                .EnableChunking(true)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            return _pulsarSystem.PulsarProducer(new CreateProducer(_schema, producerConfig)).Producer;
        }
    }
}
