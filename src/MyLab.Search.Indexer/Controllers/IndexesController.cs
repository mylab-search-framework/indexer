using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
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
            var entity = await ReadEntityFromRequestBodyAsync();

            ValidateIndexId(indexId);
            ValidateEntity(entity);

            var indexingEntity = ToIndexingRequestEntity(entity, false);

            var indexingReq = new InputIndexingRequest
            {
                IndexId = indexId,
                PostList = new []
                {
                    indexingEntity
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
            var entity = await ReadEntityFromRequestBodyAsync();

            ValidateIndexId(indexId);
            ValidateEntity(entity);

            var indexingEntity = ToIndexingRequestEntity(entity);

            var indexingReq = new InputIndexingRequest
            {
                IndexId = indexId,
                PutList = new[]
                {
                    indexingEntity
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
            var entity = await ReadEntityFromRequestBodyAsync();

            ValidateIndexId(indexId);
            ValidateEntity(entity);

            var indexingEntity = ToIndexingRequestEntity(entity);

            var indexingReq = new InputIndexingRequest
            {
                IndexId = indexId,
                PatchList = new[]
                {
                    indexingEntity
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpDelete("{indexId}/{entityId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound, "Index not found")]
        public async Task<IActionResult> Delete([FromRoute] string indexId, [FromRoute] string entityId)
        {
            ValidateIndexId(indexId);
            ValidateEntityId(entityId);

            var indexingReq = new InputIndexingRequest
            {
                IndexId = indexId,
                DeleteList = new[]
                {
                    entityId
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpPost("{indexId}/{entityId}/kicker")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound, "Index not found")]
        public async Task<IActionResult> Kick([FromRoute] string indexId, [FromRoute] string entityId)
        {
            ValidateIndexId(indexId);
            ValidateEntityId(entityId);

            var indexingReq = new InputIndexingRequest
            {
                IndexId = indexId,
                KickList = new[]
                {
                    entityId
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

        void ValidateEntityId(string entId)
        {
            if (entId == null)
                throw new ValidationException("An entity identifier was not specified");
            if (string.IsNullOrWhiteSpace(entId))
                throw new ValidationException("An entity identifier id empty");
        }

        IndexingEntity ToIndexingRequestEntity(JObject entity, bool idRequired = true)
        {
            var idProperty = entity.Property("id");
            var entId = idProperty?.Value.ToString();

            if (idRequired)
                ValidateEntityId(entId);

            var result = new IndexingEntity
            {
                Entity = entity,
                Id = string.IsNullOrWhiteSpace(entId) ? null : entId
            };

            return result;
        }

        void ValidateEntity(JObject entity)
        {
            if (entity == null)
                throw new ValidationException("An entity was not specified");

            if(entity.Count == 0)
                throw new ValidationException("An entity is empty");
        }
        private async Task<JObject> ReadEntityFromRequestBodyAsync()
        {
            JObject entity;

            using (TextReader txtRdr = new StreamReader(Request.Body))
            using (JsonReader jsonRdr = new JsonTextReader(txtRdr))
            {
                entity = await JObject.LoadAsync(jsonRdr);
            }

            return entity;
        }
    }
}
