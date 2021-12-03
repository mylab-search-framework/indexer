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
        Task IndexAsync(string entityId, string source, JobOptions jobOptions, CancellationToken cancellationToken);
    }

    class KickIndexer : IKickIndexer
    {
        private readonly IDataSourceService _dataSourceService;
        private readonly IDataIndexer _indexer;

        public KickIndexer(
            IDataSourceService dataSourceService,
            IDataIndexer indexer)
        {
            _dataSourceService = dataSourceService;
            _indexer = indexer;
        }

        public async Task IndexAsync(string entityId, string source, JobOptions jobOptions, CancellationToken cancellationToken)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (jobOptions == null) throw new ArgumentNullException(nameof(jobOptions));
            if (string.IsNullOrWhiteSpace(entityId))
                throw new BadIndexingRequestException("ID empty or not defined");
            if(jobOptions.NewUpdatesStrategy != NewUpdatesStrategy.Update)
                throw new BadIndexingRequestException("Not supported for current NewUpdatesStrategy")
                    .AndFactIs("strategy", jobOptions.NewUpdatesStrategy);

            var idParam = CreateIdParameter(jobOptions.IdPropertyName, jobOptions.IdPropertyType, entityId);
            var entBatch = await _dataSourceService.ReadByIdAsync(jobOptions.KickQuery, idParam);

            await _indexer.IndexAsync(jobOptions.JobId, entBatch.Entities, cancellationToken);
        }

        DataParameter CreateIdParameter(string idPropertyName, IdPropertyType idPropertyType, string value)
        {
            switch (idPropertyType)
            {
                case IdPropertyType.Text:
                    return DataParameter.Text(idPropertyName, value);
                case IdPropertyType.Integer:
                {
                    if (!int.TryParse(value, out var intVal))
                    {
                        throw new BadIndexingRequestException("ID value should be int32")
                            .AndFactIs("actual-value", value);
                    }

                    return DataParameter.Int32(idPropertyName, intVal);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(idPropertyType), idPropertyType, null);
            }
        }
    }
}
