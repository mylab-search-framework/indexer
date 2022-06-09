using System;
using System.Threading.Tasks;
using LinqToDB.Common;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Search.EsAdapter.Indexing;
using MyLab.Search.EsAdapter.Inter;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
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

        public Task IndexAsync(IndexingRequest req)
        {
            var idxOpts = _indexerOptions.GetIndexOptions(req.IndexId);

            if (idxOpts.EsIndex == null)
                throw new InvalidOperationException("ES index not specified")
                    .AndFactIs("index-id", req.IndexId);
            
            var bDesc = new BulkDescriptor().Index(idxOpts.EsIndex);

            if (!req.PostList.IsNullOrEmpty())
            {
                foreach (var entity in req.PostList)
                {
                    bDesc = bDesc.AddOperation(
                        new BulkCreateOperation<JObject>(entity.Entity)
                        {
                            Id = entity.Id
                        }
                    );
                }
            }

            if (!req.PutList.IsNullOrEmpty())
            {
                foreach (var entity in req.PutList)
                {
                    bDesc = bDesc.AddOperation(
                        new BulkIndexOperation<JObject>(entity.Entity)
                        {
                            Id = entity.Id
                        }
                    );
                }
            }

            if (!req.PatchList.IsNullOrEmpty())
            {
                foreach (var entity in req.PatchList)
                {
                    bDesc = bDesc.AddOperation(
                        new BulkUpdateOperation<JObject, JObject>(entity.Id)
                        {
                            Doc = entity.Entity
                        }
                    );
                }
            }

            if (!req.DeleteList.IsNullOrEmpty())
            {
                foreach (var entId in req.DeleteList)
                {
                    bDesc = bDesc.AddOperation(
                        new BulkDeleteOperation<JObject>(entId)
                    );
                }
            }

            return  _client.BulkAsync(bDesc);
        }
    }
}