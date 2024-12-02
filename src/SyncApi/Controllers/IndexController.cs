using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Nodes;
using Indexer.Application.UseCases.DeleteDocument;
using Indexer.Application.UseCases.PatchDocument;
using Indexer.Application.UseCases.PutDocument;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyLab.WebErrors;

namespace SyncApi.Controllers
{
    [ApiController]
    [Route("indexes")]
    public class IndexController(IMediator mediator) : ControllerBase
    {
        [HttpPut("{idx_id}")]
        [ErrorToResponse(typeof(FluentValidation.ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Put
        (
            [FromRoute(Name="idx_id"), Required(AllowEmptyStrings = false)] string idxId, 
            [FromBody]JsonNode document,
            CancellationToken cancellationToken
        )
        {
            await mediator.Send(new PutDocumentCommand(idxId, document), cancellationToken);

            return Ok();
        }

        [HttpPatch("{idx_id}")]
        [ErrorToResponse(typeof(FluentValidation.ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Patch
        (
            [FromRoute(Name = "idx_id"), Required(AllowEmptyStrings = false)] string idxId,
            [FromBody] JsonNode documentPart,
            CancellationToken cancellationToken
        )
        {
            await mediator.Send(new PatchDocumentCommand(idxId, documentPart), cancellationToken);

            return Ok();
        }

        [HttpPatch("{idx_id}/{doc_id}")]
        [ErrorToResponse(typeof(FluentValidation.ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete
        (
            [FromRoute(Name = "idx_id"), Required(AllowEmptyStrings = false)] string idxId,
            [FromRoute(Name = "doc_id"), Required(AllowEmptyStrings = false)] string docId,
            CancellationToken cancellationToken
        )
        {
            await mediator.Send(new DeleteDocumentCommand(idxId, docId), cancellationToken);

            return Ok();
        }
    }
}
