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
using MyLab.Search.Indexer.Options;
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

        [HttpPost("{ns}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Push([FromRoute] string ns)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            IdxOptions nsOpts;

            try
            {
                nsOpts = _options.GetIndexOptions(ns);
            }
            catch (IndexOptionsNotFoundException e)
            {
                _log.Warning(e).Write();

                return BadRequest("Index not found");
            }
            catch (NamespaceConfigException e)
            {
                _log.Warning(e).Write();

                nsOpts = e.IndexOptionsFromNamespaceOptions;
            }

            try
            {
                await _pushIndexer.IndexAsync(body, "api", nsOpts, CancellationToken.None);
            }
            catch (BadIndexingRequestException e)
            {
                _log.Warning(e).Write();
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpPost("{ns}/{id}/kick")]
        public async Task<IActionResult> Kick([FromRoute] string ns, [FromRoute] string id)
        {
            IdxOptions nsOpts;

            try
            {
                nsOpts = _options.GetIndexOptions(ns);
            }
            catch (IndexOptionsNotFoundException e)
            {
                _log.Warning(e).Write();

                return BadRequest("Index not found");
            }
            catch (NamespaceConfigException e)
            {
                _log.Warning(e).Write();

                nsOpts = e.IndexOptionsFromNamespaceOptions;
            }

            try
            {
                await _kickIndexer.IndexAsync(id, "api", nsOpts, CancellationToken.None);
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
