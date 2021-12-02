using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Controllers
{
    [Route("v1")]
    [ApiController]
    public class IndexerController : ControllerBase
    {
        private readonly IndexerOptions _options;
        private readonly IPushIndexer _pushIndexer;
        private readonly IKickIndexer _kickIndexer;
        private readonly IDslLogger _log;

        public IndexerController(
            IOptions<IndexerOptions> options,
            IPushIndexer pushIndexer,
            IKickIndexer kickIndexer,
            ILogger<IndexerController> logger)
        {
            _options = options.Value;
            _pushIndexer = pushIndexer;
            _kickIndexer = kickIndexer;
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
            {
                _log.Warning("Job not found")
                    .AndFactIs("job", job)
                    .Write();

                return BadRequest("Job not found");
            }

            try
            {
                await _pushIndexer.IndexAsync(body, "api", jobOpts, CancellationToken.None);
            }
            catch (BadIndexingRequestException e)
            {
                _log.Warning(e).Write();
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpPost("{job}/{id}/kick")]
        public async Task<IActionResult> Kick([FromRoute] string job, [FromRoute] string id)
        {
            var jobOpts = _options.Jobs.FirstOrDefault(j => j.JobId == job);
            if (jobOpts == null)
            {
                _log.Warning("Job not found")
                    .AndFactIs("job", job)
                    .Write();

                return BadRequest("Job not found");
            }

            try
            {
                await _kickIndexer.IndexAsync(id, "api", jobOpts, CancellationToken.None);
            }
            catch (BadIndexingRequestException e)
            {
                _log.Warning(e).Write();
                return BadRequest(e.Message);
            }

            return Ok();
        }
    }
}
