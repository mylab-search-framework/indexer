using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyLab.Search.Indexer.Models;
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
        public async Task<IActionResult> Post([FromRoute] string indexId, [FromBody] JObject entity)
        {
            ValidateIndexId(indexId);
            ValidateEntity(entity);

            var indexingReq = new IndexingRequest
            {
                PostList = new []
                {
                    new IndexingRequestEntity
                    {
                        Entity = entity
                    }
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpPost("{indexId}/{entityId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post([FromRoute] string indexId, [FromRoute] string entityId, [FromBody] JObject entity)
        {
            ValidateIndexId(indexId);
            ValidateEntityId(entityId);
            ValidateEntity(entity);

            var indexingReq = new IndexingRequest
            {
                PostList = new []
                {
                    new IndexingRequestEntity
                    {
                        Id = entityId,
                        Entity = entity
                    }
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpPut("{indexId}/{entityId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Put([FromRoute] string indexId, [FromRoute] string entityId, [FromBody] JObject entity)
        {
            ValidateIndexId(indexId);
            ValidateEntityId(entityId);
            ValidateEntity(entity);

            var indexingReq = new IndexingRequest
            {
                PutList = new[]
                {
                    new IndexingRequestEntity
                    {
                        Id = entityId,
                        Entity = entity
                    }
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpPatch("{indexId}/{entityId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Patch([FromRoute] string indexId, [FromRoute] string entityId, [FromBody] JObject entity)
        {
            ValidateIndexId(indexId);
            ValidateEntityId(entityId);
            ValidateEntity(entity);

            var indexingReq = new IndexingRequest
            {
                PatchList = new[]
                {
                    new IndexingRequestEntity
                    {
                        Id = entityId,
                        Entity = entity
                    }
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpDelete("{indexId}/{entityId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete([FromRoute] string indexId, [FromRoute] string entityId)
        {
            ValidateIndexId(indexId);
            ValidateEntityId(entityId);

            var indexingReq = new IndexingRequest
            {
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
        public async Task<IActionResult> Kick([FromRoute] string indexId, [FromRoute] string entityId)
        {
            ValidateIndexId(indexId);
            ValidateEntityId(entityId);

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

        void ValidateEntity(JObject entity)
        {
            if (entity == null)
                throw new ValidationException("An entity was not specified");

            if(entity.Count == 0)
                throw new ValidationException("An entity is empty");
        }
    }
}
