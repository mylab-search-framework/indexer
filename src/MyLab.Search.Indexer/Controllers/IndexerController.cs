using System;
using Microsoft.AspNetCore.Mvc;

namespace MyLab.Search.Indexer.Controllers
{
    [Route("")]
    [ApiController]
    public class IndexerController : ControllerBase
    {
        [HttpPost("{ns}")]
        [Consumes("application/json")]
        public IActionResult Push([FromRoute] string ns)
        {
            throw new NotImplementedException();
        }
    }
}
