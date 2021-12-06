using System;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB.Data;
using MyLab.Search.EsAdapter;
using MyLab.Log;

namespace MyLab.Search.Indexer.Services
{
    public interface IKickIndexer
    {
        Task IndexAsync(string entityId, string source, NsOptions nsOptions, CancellationToken cancellationToken);
    }

    class KickIndexer : IKickIndexer
    {
        private readonly INamespaceResourceProvider _nsResourceProvider;
        private readonly IDataSourceService _dataSourceService;
        private readonly IDataIndexer _indexer;

        public KickIndexer(
            INamespaceResourceProvider nsResourceProvider,
            IDataSourceService dataSourceService,
            IDataIndexer indexer)
        {
            _nsResourceProvider = nsResourceProvider;
            _dataSourceService = dataSourceService;
            _indexer = indexer;
        }

        public async Task IndexAsync(string entityId, string source, NsOptions nsOptions, CancellationToken cancellationToken)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (nsOptions == null) throw new ArgumentNullException(nameof(nsOptions));
            if (string.IsNullOrWhiteSpace(entityId))
                throw new BadIndexingRequestException("ID empty or not defined");
            if(nsOptions.NewUpdatesStrategy != NewUpdatesStrategy.Update)
                throw new BadIndexingRequestException("Not supported for current NewUpdatesStrategy")
                    .AndFactIs("strategy", nsOptions.NewUpdatesStrategy);

            var idParam = CreateIdParameter(nsOptions.IdPropertyName, nsOptions.IdPropertyType, entityId);

            string klickQuery;
            
            if (nsOptions.KickDbQuery != null)
            {
                klickQuery = nsOptions.KickDbQuery;
            }
            else
            {
                klickQuery = await _nsResourceProvider.ReadFileAsync(nsOptions.NsId, "kick.sql");
            }

            var entBatch = await _dataSourceService.ReadByIdAsync(klickQuery, idParam);

            await _indexer.IndexAsync(nsOptions.NsId, entBatch.Entities, cancellationToken);
        }

        DataParameter CreateIdParameter(string idPropertyName, IdPropertyType idPropertyType, string value)
        {
            switch (idPropertyType)
            {
                case IdPropertyType.String:
                    return DataParameter.Text(idPropertyName, value);
                case IdPropertyType.Int:
                {
                    if (!long.TryParse(value, out var intVal))
                    {
                        throw new BadIndexingRequestException("ID value should be int32")
                            .AndFactIs("actual-value", value);
                    }

                    return DataParameter.Int64(idPropertyName, intVal);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(idPropertyType), idPropertyType, null);
            }
        }
    }
}
