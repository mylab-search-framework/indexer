using Indexer.Application.Options;
using Indexer.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Indexer.Application
{
    public static class AppLogicIntegration
    {
        public const string DefaultConfigSectionName = "Indexer";

        public static IServiceCollection AddIndexerApplicationLogic(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services
                .AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Anchor>())
                .AddSingleton<EsIndexNameProvider>();
        }

        public static IServiceCollection ConfigureIndexerApplicationLogic(this IServiceCollection services, IConfiguration config, string sectionName = DefaultConfigSectionName)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (sectionName == null)
                throw new ArgumentNullException(nameof(sectionName));

            var optionsSection = config.GetSection(sectionName);
            
            services.Configure<IndexerAppOptions>(optionsSection);

            return services;
        }

        public static IServiceCollection ConfigureIndexerApplicationLogic(this IServiceCollection services, Action<IndexerAppOptions> configureOptions)
        {
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.Configure(configureOptions);

            return services;
        }
    }

    sealed class Anchor{  }
}
