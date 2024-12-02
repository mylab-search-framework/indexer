using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Nodes;
using Indexer.Application.UseCases.PutDocument;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyLab.WebErrors;

namespace SyncApi.Controllers
{
    [ApiController]
    [Route("indexes")]
    public class IndexController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IndexController> _logger;

        public IndexController(IMediator mediator, ILogger<IndexController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPut("{idx_id}")]
        [ErrorToResponse(typeof(FluentValidation.ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Put
        (
            [FromRoute(Name="idx_id"), Required(AllowEmptyStrings = false)] string idxId, 
            [FromBody]JsonNode document
        )
        {
            await _mediator.Send(new PutDocumentCommand(idxId, document));

            return Ok();
        }

        [HttpPatch("{idx_id}")]
        [ErrorToResponse(typeof(FluentValidation.ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Patch
        (
            [FromRoute(Name = "idx_id"), Required(AllowEmptyStrings = false)] string idxId,
            [FromBody] JsonNode documentPart
        )
        {
            throw new NotImplementedException();
        }

        [HttpPatch("{idx_id}/{doc_id}")]
        [ErrorToResponse(typeof(FluentValidation.ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete
        (
            [FromRoute(Name = "idx_id"), Required(AllowEmptyStrings = false)] string idxId,
            [FromRoute(Name = "doc_id"), Required(AllowEmptyStrings = false)] string docId
        )
        {
            throw new NotImplementedException();
        }
    }
}
