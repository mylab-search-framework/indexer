using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace MyLab.Search.Indexer.Api.Controllers
{
    [ApiController]
    [Route("indexes")]
    public class IndexController : ControllerBase
    {
        private readonly ILogger<IndexController> _logger;

        public IndexController(ILogger<IndexController> logger)
        {
            _logger = logger;
        }

        [HttpPut("{idx_id}/{doc_id}")]
        public async Task<IActionResult> Put
        (
            [FromRoute(Name="idx_id"), Required(AllowEmptyStrings = false)] string idxId, 
            [FromRoute(Name = "doc_id"), Required(AllowEmptyStrings = false)] string docId, 
            [FromBody]JsonObject document
        )
        {

        }

        [HttpPatch("{idx_id}/{doc_id}")]
        public async Task<IActionResult> Patch
        (
            [FromRoute(Name = "idx_id"), Required(AllowEmptyStrings = false)] string idxId,
            [FromRoute(Name = "doc_id"), Required(AllowEmptyStrings = false)] string docId,
            [FromBody] JsonObject documentPart
        )
        {

        }

        [HttpPatch("{idx_id}/{doc_id}")]
        public async Task<IActionResult> Delete
        (
            [FromRoute(Name = "idx_id"), Required(AllowEmptyStrings = false)] string idxId,
            [FromRoute(Name = "doc_id"), Required(AllowEmptyStrings = false)] string docId
        )
        {

        }
    }
}
