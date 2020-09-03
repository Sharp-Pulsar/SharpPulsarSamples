using Akka.Actor;
using Echo.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Handlers;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using System;

namespace Pulsar
{
    public class PulsarConnector
    {
        public PulsarSystem PulsarSystem;
        public IActorRef Producer;
        private IConfiguration _configuration;
        private string _contentRoot;
        private AvroSchema _schema;
        private PulsarSettings _pulsarSettings;
        public void Connect(PulsarSettings pulsarSettings, IWebHostEnvironment env)
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

            PulsarSystem = PulsarSystem.GetInstance(clientConfig);
            _schema = AvroSchema.Of(typeof(Echo.Common.Echo));
            Producer = CreateProducer();
        }
        private IActorRef CreateProducer()
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

            return PulsarSystem.PulsarProducer(new CreateProducer(_schema, producerConfig)).Producer;
        }
    }
}
