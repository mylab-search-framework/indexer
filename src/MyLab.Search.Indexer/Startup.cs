using LinqToDB.DataProvider;
using LinqToDB.DataProvider.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLab.Db;
using MyLab.HttpMetrics;
using MyLab.Log;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Queue;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using MyLab.StatusProvider;
using MyLab.WebErrors;
using Prometheus;

namespace MyLab.Search.Indexer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(opt => opt.AddExceptionProcessing());

            services
                .AddSingleton<IIndexResourceProvider, FileIndexResourceProvider>()
                .AddSingleton<IDataSourceService, DbDataSourceService>()
                .AddSingleton<ISeedService, FileSeedService>()
                .AddRabbit()
                .AddRabbitConsumers<IndexerRabbitRegistrar>()
                .AddDbTools<ConfiguredDataProviderSource>(Configuration)
                .AddAppStatusProviding()
                .AddEsTools(Configuration, "ES")
                .AddLogging(l => l.AddMyLabConsole())
                .AddUrlBasedHttpMetrics();

            services
                .ConfigureRabbit(Configuration)
                .Configure<IndexerOptions>(Configuration.GetSection("Indexer"))
                .Configure<IndexerDbOptions>(Configuration.GetSection("DB"))

#if DEBUG
                .Configure<ExceptionProcessingOptions>(opt => opt.HideError = false);
#else
                ;
#endif

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapMetrics();
                })
                .UseStatusApi();
        }
    }
}
