using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Services
{
    class InputRequestProcessor : IInputRequestProcessor
    {
        private readonly IDataSourceService _dataSourceService;
        private readonly IIndexerService _indexerService;
        private readonly IndexerOptions _options;

        public InputRequestProcessor(
            IDataSourceService dataSourceService, 
            IIndexerService indexerService,
            IOptions<IndexerOptions> options)
        :this(dataSourceService, indexerService, options.Value)
        {
        }

        public InputRequestProcessor(
            IDataSourceService dataSourceService,
            IIndexerService indexerService,
            IndexerOptions options)
        {
            _dataSourceService = dataSourceService;
            _indexerService = indexerService;
            _options = options;
        }

        public async Task IndexAsync(InputIndexingRequest inputRequest)
        {
            var indexOptions = _options.GetIndexOptions(inputRequest.IndexId);

            var idxReq = inputRequest.Clone();

            if (inputRequest.KickList is { Length: > 0 })
            {
                var entities = await _dataSourceService.LoadByIdListAsync(inputRequest.IndexId, inputRequest.KickList);

                if (entities != null)
                {
                    if (indexOptions.IsStream)
                    {
                        idxReq.PostList = JoinEntities(idxReq.PostList, entities);
                    }
                    else
                    {
                        idxReq.PutList = JoinEntities(idxReq.PutList, entities);
                    }
                }
            }

            await _indexerService.IndexEntities(idxReq);
        }

        IndexingRequestEntity[] JoinEntities(IndexingRequestEntity[] arr1, IndexingRequestEntity[] arr2)
        {
            if (arr1 == null || arr1.Length == 0) return arr2;
            if (arr2 == null || arr2.Length == 0) return arr1;

            var newList = new List<IndexingRequestEntity>(arr1);
            newList.AddRange(arr2);

            return newList.ToArray();
        }
    }
}