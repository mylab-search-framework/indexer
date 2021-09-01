using System;
using MyLab.RabbitClient;
using MyLab.RabbitClient.Connection;
using MyLab.RabbitClient.Model;

namespace FunctionTests
{
    public class MqFixture : IDisposable
    {
        private readonly RabbitConnectionProvider _connection;
        private readonly RabbitQueueFactory _qFactory;

        public MqFixture()
        {
            _connection = new RabbitConnectionProvider(new RabbitOptions
            {
                Host = "localhost",
                Password = "guest",
                User = "guest"
            });       
            var channelProvider = new RabbitChannelProvider(_connection);
            _qFactory = new RabbitQueueFactory(channelProvider)
            {
                AutoDelete = true,
                Prefix = "test-"
            };
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        public RabbitQueue CreateWithRandomId()
        {
            return _qFactory.CreateWithRandomId();
        }
    }
}