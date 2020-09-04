using Akka.Actor;
using Echo.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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

namespace Pulsar
{
    public class PulsarConnector
    {
        private PulsarSystem _pulsarSystem;
        public static IActorRef _producer;
        private string _contentRoot;
        private AvroSchema _schema;
        private PulsarSettings _pulsarSettings;
        public PulsarConnector(PulsarSettings pulsarSettings, IWebHostEnvironment env)
        {
            _pulsarSettings = pulsarSettings;
            _contentRoot = env.ContentRootPath;
        }
        public void Start()
        {           
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl(_pulsarSettings.ServiceUrl)
                .ConnectionsPerBroker(1)
                .UseProxy(_pulsarSettings.UseProxy)
                .OperationTimeout(_pulsarSettings.OperationTimeout)
                .AllowTlsInsecureConnection(false)
                .ProxyServiceUrl(_pulsarSettings.ProxyServiceUrl, ProxyProtocol.SNI)
                .Authentication(new AuthenticationDisabled())
                .ClientConfigurationData;

            _pulsarSystem = PulsarSystem.GetInstance(clientConfig);
            _schema = AvroSchema.Of(typeof(Echo.Common.Echo));
            _producer = CreateProducer();
        }
        public SentReceipt Send(Send send)
        {
            return _pulsarSystem.Send(send, _producer);
        }
        public  void Stop()
        {
            _pulsarSystem.Stop();
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

            return _pulsarSystem.PulsarProducer(new CreateProducer(_schema, producerConfig)).Producer;
        }
    }
}
