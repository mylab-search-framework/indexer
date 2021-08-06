using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyLab.Db;
using MyLab.Mq.PubSub;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using MyLab.StatusProvider;
using MyLab.TaskApp;

namespace MyLab.Search.Indexer
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of <see cref="Startup"/>
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton(_configuration)
                .Configure<IndexerOptions>(_configuration.GetSection("Indexer"))
                .Configure<IndexerMqOptions>(_configuration.GetSection("MQ"))
                .Configure<IndexerDbOptions>(_configuration.GetSection("DB"))
                .ConfigureMq(_configuration, "MQ");

            services
                .AddTaskLogic<IndexerTaskLogic>()
                .AddAppStatusProviding()
                .AddDbTools<ConfiguredDataProviderSource>(_configuration)
                .AddMqConsuming(cReg => cReg.RegisterConsumerByOptions<IndexerMqOptions>(
                        opt => new MqConsumer<string, IndexerMqConsumerLogic>(opt.Queue))
                , optional: true)
                .AddEsTools(_configuration, "ES")
                
                .AddLogging(l => l.AddConsole())
                .AddSingleton<ISeedService, FileSeedService>()
                .AddSingleton<IDataIndexer, DataIndexer>()
                .AddSingleton<IDataSourceService, DbDataSourceService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting()
                .UseTaskApi()
                .UseStatusApi();

        }
    }
}
