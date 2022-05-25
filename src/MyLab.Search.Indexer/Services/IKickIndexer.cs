using System;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB.Data;
using MyLab.Search.EsAdapter;
using MyLab.Log;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Services
{
    public interface IKickIndexer
    {
        Task IndexAsync(string entityId, string source, IdxOptions idxOptions, CancellationToken cancellationToken);
    }

    class KickIndexer : IKickIndexer
    {
        private readonly IIndexResourceProvider _nsResourceProvider;
        private readonly IDataSourceService _dataSourceService;
        private readonly IDataIndexer _indexer;

        public KickIndexer(
            IIndexResourceProvider nsResourceProvider,
            IDataSourceService dataSourceService,
            IDataIndexer indexer)
        {
            _nsResourceProvider = nsResourceProvider;
            _dataSourceService = dataSourceService;
            _indexer = indexer;
        }

        public async Task IndexAsync(string entityId, string source, IdxOptions idxOptions, CancellationToken cancellationToken)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (idxOptions == null) throw new ArgumentNullException(nameof(idxOptions));
            if (string.IsNullOrWhiteSpace(entityId))
                throw new BadIndexingRequestException("ID empty or not defined");
            if(idxOptions.NewUpdatesStrategy != NewUpdatesStrategy.Update)
                throw new BadIndexingRequestException("Not supported for current NewUpdatesStrategy")
                    .AndFactIs("strategy", idxOptions.NewUpdatesStrategy);

            var idParam = CreateIdParameter(idxOptions.IdPropertyName, idxOptions.IdPropertyType, entityId);

            string kickQuery;
            
            if (idxOptions.KickDbQuery != null)
            {
                kickQuery = idxOptions.KickDbQuery;
            }
            else
            {
                kickQuery = await _nsResourceProvider.ReadFileAsync(idxOptions.Id, "kick.sql");
            }

            var entBatch = await _dataSourceService.ReadByIdAsync(kickQuery, idParam);

            await _indexer.IndexAsync(idxOptions.Id, entBatch.Entities, cancellationToken);
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
