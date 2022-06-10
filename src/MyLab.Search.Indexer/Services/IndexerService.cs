using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Search.EsAdapter.Indexing;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;
using Nest;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Services
{
    class IndexerService : IIndexerService
    {
        private readonly IndexerOptions _indexerOptions;
        private readonly IEsIndexer _esIndexer;

        public IndexerService(IEsIndexer esIndexer, IOptions<IndexerOptions> idxOptions)
            :this(esIndexer, idxOptions.Value)
        {
        }

        public IndexerService(IEsIndexer esIndexer, IndexerOptions indexerOptions)
        {
            _indexerOptions = indexerOptions;
            _esIndexer = esIndexer;
        }

        public async Task IndexAsync(IndexingRequest req)
        {
            var idxOpts = _indexerOptions.GetIndexOptions(req.IndexId);

            if (idxOpts.EsIndex == null)
                throw new InvalidOperationException("ES index not specified")
                    .AndFactIs("index-id", req.IndexId);

            var bulkReq = new Func<BulkDescriptor, IBulkRequest>(d =>
                {
                    if (req.PostList != null && req.PostList.Length != 0)
                    {
                        d = req.PostList.Aggregate(
                            d, 
                            (curD, doc) => curD.Create<JObject>(
                                cd => cd
                                    .Document(doc)
                                    .Id(doc.GetIdProperty())
                                )
                            );
                    }

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

            await _esIndexer.BulkAsync<JObject>(idxOpts.EsIndex, bulkReq);
        }
    }
}