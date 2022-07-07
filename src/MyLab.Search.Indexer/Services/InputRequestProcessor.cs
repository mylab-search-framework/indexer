using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using Newtonsoft.Json.Linq;
using SQLitePCL;

namespace MyLab.Search.Indexer.Services
{
    class InputRequestProcessor : IInputRequestProcessor
    {
        private readonly IDataSourceService _dataSourceService;
        private readonly IIndexerService _indexerService;
        private readonly IndexerOptions _options;
        private readonly IDslLogger _log;

        public InputRequestProcessor(
            IDataSourceService dataSourceService, 
            IIndexerService indexerService,
            IOptions<IndexerOptions> options,
            ILogger<InputRequestProcessor> logger = null)
        :this(dataSourceService, indexerService, options.Value, logger)
        {
        }

        public InputRequestProcessor(
            IDataSourceService dataSourceService,
            IIndexerService indexerService,
            IndexerOptions options,
            ILogger<InputRequestProcessor> logger = null)
        {
            _dataSourceService = dataSourceService;
            _indexerService = indexerService;
            _options = options;
            _log = logger?.Dsl();
        }

        public async Task IndexAsync(InputIndexingRequest inputRequest)
        {
            var idxReq = inputRequest.Clone();

            if (inputRequest.KickList is { Length: > 0 })
            {
                var docsLoad = await _dataSourceService.LoadKickAsync(inputRequest.IndexId, inputRequest.KickList);

                _log?.Debug("Kick list has been loaded")
                    .AndFactIs("count", docsLoad.Batch.Docs.Length)
                    .AndFactIs("query", docsLoad.Batch.Query)
                    .Write();

                if (docsLoad is { Batch: { Docs: { Length: > 0 } } })
                {
                    var docs = docsLoad.Batch.Docs.ToArray();

                    if (docs.Length != inputRequest.KickList.Length)
                    {
                        throw new KickDocsCountMismatchException();
                    }

                    var totalIndexType = _options.GetTotalIndexType(inputRequest.IndexId);

                    switch (totalIndexType)
                    {
                        case IndexType.Heap:
                            {
                                idxReq.PutList = JoinDocs(idxReq.PutList, docs);
                            }
                            break;
                        case IndexType.Stream:
                            {
                                idxReq.PostList = JoinDocs(idxReq.PostList, docs);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            await _indexerService.IndexAsync(idxReq);
        }

        JObject[] JoinDocs(JObject[] arr1, JObject[] arr2)
        {
            if (arr1 == null || arr1.Length == 0) return arr2;
            if (arr2 == null || arr2.Length == 0) return arr1;

            var newList = new List<JObject>(arr1);
            newList.AddRange(arr2);

            return newList.ToArray();
        }
    }
}