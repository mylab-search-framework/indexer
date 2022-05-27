using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLab.Log;
using MyLab.Search.Indexer.Queue;
using MyLab.WebErrors;

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

#if DEBUG
            services.Configure<ExceptionProcessingOptions>(opt => opt.HideError = false);
#endif

            services.Configure<IndexerOptions>(Configuration.GetSection("Indexer"));
            services.AddLogging(l => l.AddMyLabConsole());
            services
                .AddRabbit()
                .ConfigureRabbit(Configuration)
                .AddRabbitConsumers<IndexerRabbitRegistrar>();
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
            });
        }
    }
}
