using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Controllers
{
    [Route("v1")]
    [ApiController]
    public class IndexerController : ControllerBase
    {
        private readonly IndexerOptions _options;
        private readonly IPushIndexer _pushIndexer;
        private readonly IDslLogger _log;

        public IndexerController(
            IOptions<IndexerOptions> options,
            IPushIndexer pushIndexer,
            ILogger<IndexerController> logger)
        {
            _options = options.Value;
            _pushIndexer = pushIndexer;
            _log = logger.Dsl();
        }

        [HttpPost("{job}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Push([FromRoute] string job)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var jobOpts = _options.Jobs.FirstOrDefault(j => j.JobId == job);
            if (jobOpts == null)
                throw new InvalidOperationException("Job not found for queue");

            try
            {
                await _pushIndexer.Index(body, "api", jobOpts);
            }
            catch (InputEntityValidationException e)
            {
                _log.Warning(e).Write();
                return BadRequest(e.Message);
            }

            return Ok();
        }
    }
}
