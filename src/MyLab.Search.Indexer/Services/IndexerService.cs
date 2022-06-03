using System;
using System.Threading.Tasks;
using LinqToDB.Common;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using Nest;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Services
{
    class IndexerService : IIndexerService
    {
        private readonly IndexerOptions _indexerOptions;
        private readonly ElasticClient _client;

        public IndexerService(IEsClientProvider clientProvider, IOptions<IndexerOptions> idxOptions)
            :this(clientProvider, idxOptions.Value)
        {
        }

        public IndexerService(IEsClientProvider clientProvider, IndexerOptions indexerOptions)
        {
            _indexerOptions = indexerOptions;
            _client = clientProvider.Provide();
        }

        public Task IndexEntities(IndexingRequest req)
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