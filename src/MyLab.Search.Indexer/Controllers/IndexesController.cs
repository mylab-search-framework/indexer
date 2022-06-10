using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using MyLab.WebErrors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Controllers
{
    [Route("v2/indexes")]
    [ApiController]
    public class IndexesController : ControllerBase
    {
        private readonly IInputRequestProcessor _inputRequestProcessor;

        public IndexesController(IInputRequestProcessor inputRequestProcessor)
        {
            _inputRequestProcessor = inputRequestProcessor;
        }

        [HttpPost("{indexId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound, "Index not found")]
        public async Task<IActionResult> Post([FromRoute] string indexId)
        {
            var doc = await ReadDocFromRequestBodyAsync();

            ValidateIndexId(indexId);
            ValidateDoc(doc);

            var indexingReq = new InputIndexingRequest
            {
                IndexId = indexId,
                PostList = new []
                {
                    doc
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpPut("{indexId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound, "Index not found")]
        public async Task<IActionResult> Put([FromRoute] string indexId)
        {
            var doc = await ReadDocFromRequestBodyAsync();

            ValidateIndexId(indexId);
            ValidateDoc(doc);

            var indexingReq = new InputIndexingRequest
            {
                IndexId = indexId,
                PutList = new[]
                {
                    doc
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpPatch("{indexId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound, "Index not found")]
        public async Task<IActionResult> Patch([FromRoute] string indexId)
        {
            var doc = await ReadDocFromRequestBodyAsync();

            ValidateIndexId(indexId);
            ValidateDoc(doc);
            
            var indexingReq = new InputIndexingRequest
            {
                IndexId = indexId,
                PatchList = new[]
                {
                    doc
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpDelete("{indexId}/{docId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound, "Index not found")]
        public async Task<IActionResult> Delete([FromRoute] string indexId, [FromRoute] string docId)
        {
            ValidateIndexId(indexId);
            ValidateDocId(docId);

            var indexingReq = new InputIndexingRequest
            {
                IndexId = indexId,
                DeleteList = new[]
                {
                    docId
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpPost("{indexId}/{docId}/kicker")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound, "Index not found")]
        public async Task<IActionResult> Kick([FromRoute] string indexId, [FromRoute] string docId)
        {
            ValidateIndexId(indexId);
            ValidateDocId(docId);

            var indexingReq = new InputIndexingRequest
            {
                IndexId = indexId,
                KickList = new[]
                {
                    docId
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        void ValidateIndexId(string idxId)
        {
            if (idxId == null)
                throw new ValidationException("An index identifier was not specified");
            if (string.IsNullOrWhiteSpace(idxId))
                throw new ValidationException("An index identifier id empty");
        }

        void ValidateDocId(string entId)
        {
            if (entId == null)
                throw new ValidationException("An doc identifier was not specified");
            if (string.IsNullOrWhiteSpace(entId))
                throw new ValidationException("An doc identifier id empty");
        }

        void ValidateDoc(JObject doc)
        {
            if (doc == null)
                throw new ValidationException("An doc was not specified");

            if(doc.Count == 0)
                throw new ValidationException("An doc is empty");

            doc.CheckIdProperty();
        }
        private async Task<JObject> ReadDocFromRequestBodyAsync()
        {
            JObject doc;

            using (TextReader txtRdr = new StreamReader(Request.Body))
            using (JsonReader jsonRdr = new JsonTextReader(txtRdr))
            {
                doc = await JObject.LoadAsync(jsonRdr);
            }

            return doc;
        }
    }
}
