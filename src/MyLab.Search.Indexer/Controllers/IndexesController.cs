using Microsoft.AspNetCore.Mvc;
using System.Net;
using MediatR;
using MyLab.Search.Indexer.Handlers.IndexingRequest;
using MyLab.WebErrors;
using IndexingObject = MyLab.Search.Indexer.Model.IndexingObject;
using LiteralId = MyLab.Search.Indexer.Model.LiteralId;
using ValidationException = FluentValidation.ValidationException;

namespace MyLab.Search.Indexer.Controllers
{

    [Route("v2/indexes")]
    [ApiController]
    public class IndexesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IndexesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("{indexId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound)]
        public async Task<IActionResult> Put([FromRoute] LiteralId indexId, [FromBody] IndexingObject document)
        {
            await _mediator.Send(new IndexingRequestCommand
            {
                IndexId = indexId,
                PutList = new[]{ document }
            });

            return Ok();
        }

        [HttpPatch("{indexId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound)]
        public async Task<IActionResult> Patch([FromRoute] LiteralId indexId, [FromBody] IndexingObject document)
        {
            await _mediator.Send(new IndexingRequestCommand
            {
                IndexId = indexId,
                PatchList = new[] { document }
            });

            return Ok();
        }

        [HttpDelete("{indexId}/{docId}")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete([FromRoute] LiteralId indexId, [FromRoute] LiteralId docId)
        {
            await _mediator.Send(new IndexingRequestCommand
            {
                IndexId = indexId,
                DeleteList = new []{docId}
            });

            return Ok();
        }

        [HttpPost("{indexId}/{docId}/kicker")]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [ErrorToResponse(typeof(IndexOptionsNotFoundException), HttpStatusCode.NotFound)]
        public async Task<IActionResult> Kick([FromRoute] LiteralId indexId, [FromRoute] LiteralId docId)
        {
            await _mediator.Send(new IndexingRequestCommand
            {
                IndexId = indexId,
                KickList= new[] { docId }
            });

            return Ok();
        }
    }
}
