using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter.Indexing;
using MyLab.Search.EsAdapter.Inter;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;
using Nest;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;
using YamlDotNet.Core.Tokens;

namespace MyLab.Search.Indexer.Services
{
    public interface IIndexerService
    {
        Task IndexAsync(IndexingRequest req, CancellationToken cToken = default);
    }
    class IndexerService : IIndexerService
    {
        private readonly IndexerOptions _indexerOptions;
        private readonly IEsIndexer _esIndexer;
        private readonly IIndexCreator _indexerCreator;
        private readonly IDslLogger _log;

        public IndexerService(
            IEsIndexer esIndexer,
            IOptions<IndexerOptions> idxOptions,
            IIndexCreator indexerCreator = null,
            ILogger<IndexerService> logger = null)
            : this(esIndexer, idxOptions.Value, indexerCreator, logger)
        {
        }

        public IndexerService(
            IEsIndexer esIndexer,
            IndexerOptions indexerOptions,
            IIndexCreator indexerCreator = null,
            ILogger<IndexerService> logger = null)
        {
            _indexerOptions = indexerOptions;
            _esIndexer = esIndexer;
            _indexerCreator = indexerCreator;
            _log = logger?.Dsl();
        }

        public async Task IndexAsync(IndexingRequest req, CancellationToken cToken = default)
        {
            if (req.IsEmpty())
            {
                throw new InvalidOperationException("Indexing request is empty");
            }

            var bulkReq = new Func<BulkDescriptor, IBulkRequest>(d =>
            {
                if (req.PutList != null && req.PutList.Length != 0)
                {
                    d = req.PutList.Aggregate(
                        d,
                        (curD, doc) => curD.Index<JObject>(
                            cd => cd
                                .Document(doc)
                                .Id(doc.GetIdProperty())
                        )
                    );
                }

                if (req.PatchList != null && req.PatchList.Length != 0)
                {
                    d = req.PatchList.Aggregate(
                        d,
                        (curD, doc) => curD.Update<JObject>(
                            cd => cd
                                .Doc(doc)
                                .Id(doc.GetIdProperty())
                        )
                    );
                }

                if (req.DeleteList != null && req.DeleteList.Length != 0)
                {
                    d = req.DeleteList.Aggregate(
                        d,
                        (curD, docId) => curD.Delete<JObject>(
                            cd => cd.Id(docId)
                        )
                    );
                }

                return d;
            }
            );

            var esIdxName = _indexerOptions.GetEsName(req.IndexId);

            try
            { 
                var bulkResponse = await _esIndexer.BulkAsync<JObject>(esIdxName, bulkReq, cToken);

                if (HasIndexNotFound(bulkResponse))
                {
                    await Retry();
                }
            }
            catch (EsException e) when (e.Response.HasIndexNotFound)
            {
                await Retry();
            }


            async Task Retry()
            {
                await CreateIndexWhenDoesNotExists(esIdxName, req.IndexId, cToken);

                _log?.Action("Try to index docs after index created").Write();

                var resp = await _esIndexer.BulkAsync<JObject>(esIdxName, bulkReq, cToken);

                if (HasIndexNotFound(resp))
                    throw new InvalidOperationException("Can't create index")
                        .AndFactIs("idx-name", esIdxName);
            }
        }

        bool HasIndexNotFound(BulkResponse response)
            => !response.IsValid && (response.ServerError?.Error?.Type == "index_not_found_exception" || response.Items.Any(i => i.Error?.Type == "index_not_found_exception"));

        async Task CreateIndexWhenDoesNotExists(string esIdxName, string indexId, CancellationToken cToken)
        {
            _log?.Warning("Index not found. Its try to create index.")
                .AndFactIs("idx-name", esIdxName)
                .Write();

            if (_indexerCreator == null)
                throw new InvalidOperationException("Indexer creator not found");

            await _indexerCreator.CreateIndexAsync(indexId, cToken);
            await Task.Delay(500, cToken);
        }
    }
}
