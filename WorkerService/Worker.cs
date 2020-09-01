using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Handlers;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using WorkerService.Hubs;
using Echo.Common;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHubContext<EchoHub> _echo;

        private readonly PulsarSystem _pulsarSystem;
        private IActorRef _producer;
        private IActorRef _consumer;
        private string _contentRoot;
        private AvroSchema _schema;
        private string _topic;
        public Worker(ILogger<Worker> logger, IHubContext<EchoHub> echo, PulsarSettings pulsarSettings)
        {
            _echo = echo;
            _logger = logger; 
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl(pulsarSettings.ServiceUrl)
                .ConnectionsPerBroker(1)
                .UseProxy(pulsarSettings.UseProxy)
                .OperationTimeout(pulsarSettings.OperationTimeout)
                .AllowTlsInsecureConnection(false)
                .ProxyServiceUrl(pulsarSettings.ProxyServiceUrl, ProxyProtocol.SNI)
                .Authentication(new AuthenticationDisabled())
                .ClientConfigurationData;
            _topic = pulsarSettings.Topic;
            _pulsarSystem = PulsarSystem.GetInstance(clientConfig);
            _schema = AvroSchema.Of(typeof(Echo.Common.Echo));
            _consumer = Consumer(pulsarSettings);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try
                {
                    var message = _pulsarSystem.Receive(_topic);
                    if(message != null)
                    {
                        var echo = message.Message.ToTypeOf<Echo.Common.Echo>();
                        await _echo.Clients.All.SendAsync("echo", JsonSerializer.Serialize(echo, new JsonSerializerOptions { WriteIndented = true }));
                        _pulsarSystem.Acknowledge(message);
                    }
                }
                catch(Exception ex)
                {
                    await _echo.Clients.All.SendAsync("error", ex.ToString());
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
        private IActorRef Consumer(PulsarSettings pulsarSettings)
        {
            var consumerListener = new DefaultConsumerEventListener(l=> { });
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(pulsarSettings.Topic)
                .ForceTopicCreation(true)
                .SubscriptionName($"{pulsarSettings.Topic}-Subscription")
                .Topic(pulsarSettings.Topic)
                //.AckTimeout(10000)
                .AcknowledgmentGroupTime(0)
                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(_schema)
                .SetConsumptionType(ConsumptionType.Queue)

                .MessageListener(new DefaultMessageListener(null, null))
                .StartMessageId(MessageIdFields.Earliest)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            return _pulsarSystem.PulsarConsumer(new CreateConsumer(_schema, consumerConfig)).Consumer;
        }
    }
}
