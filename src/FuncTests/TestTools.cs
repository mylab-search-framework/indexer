using MyLab.RabbitClient;
using MyLab.RabbitClient.Connection;

namespace FuncTests
{
    static class TestTools
    {
        public const string EsUrl = "http://localhost:9200";

        public static IRabbitChannelProvider RabbitChannelProvider { get; }
        
        static TestTools()
        {
            var rabbitConnectionProvider = new LazyRabbitConnectionProvider(new RabbitOptions
            {
                Host = "localhost",
                User = "guest",
                Password = "guest"
            });
            RabbitChannelProvider = new RabbitChannelProvider(rabbitConnectionProvider);
        }
    }
}