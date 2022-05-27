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
    [Route("v2")]
    [ApiController]
    public class IndexingController : ControllerBase
    {
        private readonly IInputRequestProcessor _inputRequestProcessor;

        public IndexingController(IInputRequestProcessor inputRequestProcessor)
        {
            _inputRequestProcessor = inputRequestProcessor;
        }

        [HttpPost]
        [ErrorToResponse(typeof(ValidationException), HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post(/*[FromBody] IndexingRequest request*/)
        {
            IndexingRequest request;

            using (TextReader txtRdr = new StreamReader(Request.Body))
            {
                var reqTxt = await txtRdr.ReadToEndAsync();
                request = JsonConvert.DeserializeObject<IndexingRequest>(reqTxt);
            }

            await _inputRequestProcessor.ProcessRequestAsync(request);

            return Ok();
        }
    }
}
