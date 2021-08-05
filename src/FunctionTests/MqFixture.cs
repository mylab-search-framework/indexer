using System;
using MyLab.Mq;
using MyLab.Mq.Communication;
using MyLab.Mq.MqObjects;

namespace FunctionTests
{
    public class MqFixture : MqQueueFactoryBase, IDisposable
    {
        private readonly DefaultMqConnectionProvider _connection;
        private readonly DefaultMqChannelProvider _channelProvider;

        public MqFixture()
        {
            _connection = new DefaultMqConnectionProvider(new MqOptions
            {
                Host = "localhost",
                Password = "guest",
                User = "guest"
            });       
            _channelProvider = new DefaultMqChannelProvider(_connection);
        }

        protected override IMqChannelProvider GetChannelProvider()
        {
            return _channelProvider;
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _channelProvider?.Dispose();
        }
    }
}