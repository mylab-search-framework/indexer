using Microsoft.AspNetCore.Mvc;
using System.Net;
using FluentValidation;
using Indexer.Domain.Model;
using Indexer.Domain.Validators;
using MyLab.WebErrors;
using IndexingObject = MyLab.Search.Indexer.Model.IndexingObject;
using IndexingObjectValidator = MyLab.Search.Indexer.Validators.IndexingObjectValidator;
using IndexingRequest = MyLab.Search.Indexer.Model.IndexingRequest;
using LiteralId = MyLab.Search.Indexer.Model.LiteralId;

namespace MyLab.Search.Indexer.Controllers
{
    [Route("v2/indexes")]
    [ApiController]
    public class IndexesController : ControllerBase
    {
        [HttpPut("{indexId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound)]
        public async Task<IActionResult> Put([FromRoute] LiteralId indexId, [FromBody] IndexingObject indexingObject)
        {
            var indexingReq = new IndexingRequest
            {
                IndexId = indexId,
                PutList = new[]
                {
                    indexingObject
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpPatch("{indexId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound)]
        public async Task<IActionResult> Patch([FromRoute] LiteralId indexId, [FromBody] IndexingObject indexingObject)
        {
            await _indexIdValidator.ValidateAsync(indexId);

            var validator = new IndexingObjectValidator();
            await validator.ValidateAndThrowAsync(indexingObject);

            var indexingReq = new IndexingRequest
            {
                IndexId = indexId,
                PatchList = new[]
                {
                    indexingObject
                }
            };

            await _inputRequestProcessor.IndexAsync(indexingReq);

            return Ok();
        }

        [HttpDelete("{indexId}/{docId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete([FromRoute] LiteralId indexId, [FromRoute] LiteralId docId)
        {
            var indexingReq = new IndexingRequest
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
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound)]
        public async Task<IActionResult> Kick([FromRoute] LiteralId indexId, [FromRoute] string docId)
        {
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
    }
}
