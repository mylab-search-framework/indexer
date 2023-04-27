using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Log.Scopes;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;
using Nest;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class LifecyclePolicyUploader : IResourceUploader
    {
        private readonly IEsTools _esTools;
        private readonly IResourceProvider _resourceProvider;
        private readonly IDslLogger _log;
        private readonly IndexerOptions _opts;

        public LifecyclePolicyUploader(
            IEsTools esTools, 
            IResourceProvider resourceProvider,
            IOptions<IndexerOptions> options,
            ILogger<LifecyclePolicyUploader> logger = null)
        {
            _esTools = esTools;
            _resourceProvider = resourceProvider;
            _log = logger?.Dsl();
            _opts = options.Value;
        }
        public async Task UploadAsync(CancellationToken cancellationToken)
        {
            var policies = _resourceProvider.ProvideLifecyclePolicies();

            _log?.Action("Lifecycle policies uploading")
                .AndFactIs("policy-list", policies.Select(p => p.Name).ToArray())
                .Write();

            foreach (var policy in policies)
            {
                using (_log?.BeginScope(new FactLogScope("policy", policy.Name)))
                {
                    await TryUploadPolicyAsync(policy, cancellationToken);
                }
            }
        }

        private async Task TryUploadPolicyAsync(IResource policy, CancellationToken cancellationToken)
        {
            var policyId = _opts.GetEsName(policy.Name);

            try
            {
                await using var readStream = policy.OpenRead();
                var resPolicy = _esTools.Deserializer.DeserializeLifecyclePolicy(readStream);

                if (resPolicy.Policy == null)
                {
                    _log?.Error("Policy resource has no 'policy' node").Write();

                    return;
                }

                resPolicy.Policy.Meta ??= new Dictionary<string, object>();
                var resPolicyMetadata = ServiceMetadata.Extract(resPolicy.Policy.Meta);

                var esPolicy = await _esTools.LifecyclePolicy(policyId).TryGetAsync(cancellationToken);

                if (esPolicy == null)
                { 
                    _log?.Action("Policy not found in ES and will be uploaded").Write();

                    var newInitialMetadata = new ServiceMetadata
                    {
                        Creator = ServiceMetadata.MyCreator,
                        Ver = resPolicyMetadata?.Ver,
                        History = new ServiceMetadata.HistoryItem[]
                        {
                            new ()
                            {
                                ComponentVer = resPolicyMetadata?.Ver,
                                ActorVer = GetMyActorVersion(),
                                ActDt = DateTime.Now,
                                Actor = ServiceMetadata.MyCreator
                            }
                        }
                    };

                    newInitialMetadata.Save(resPolicy.Policy.Meta);

                    await UploadPolicyAsync(policyId, resPolicy, cancellationToken);

                    _log?.Action("Policy was uploaded").Write();

                    return;
                }

                var esPolicyMetadata = ServiceMetadata.Extract(esPolicy.Policy?.Meta);

                if (esPolicyMetadata == null || !esPolicyMetadata.IsMyCreator())
                {
                    _log?.Warning("The same policy from another creator was found")
                        .AndFactIs("creator", esPolicyMetadata?.Creator)
                        .Write();

                    return;
                }

                if (resPolicyMetadata?.Ver == esPolicyMetadata.Ver)
                {
                    _log?.Action("Upload canceled due to actual policy version")
                        .AndFactIs("ver", resPolicyMetadata?.Ver)
                        .Write();

                    return;
                }

                var newHistory = new List<ServiceMetadata.HistoryItem>();

                if (esPolicyMetadata.History is { Length: > 0 })
                {
                    newHistory.AddRange(esPolicyMetadata.History);
                }

                newHistory.Add(new ServiceMetadata.HistoryItem
                {
                    Actor = ServiceMetadata.MyCreator,
                    ActDt = DateTime.Now,
                    ActorVer = GetMyActorVersion(),
                    ComponentVer = resPolicyMetadata?.Ver
                });

                var newMetadata = new ServiceMetadata
                {
                    Creator = esPolicyMetadata.Creator,
                    Ver = resPolicyMetadata?.Ver,
                    History = newHistory.Count > 0 ? newHistory.ToArray() : null
                };

                newMetadata.Save(resPolicy.Policy.Meta);

                await UploadPolicyAsync(policyId, resPolicy, cancellationToken);
            }
            catch (Exception e)
            {
                _log.Error("Unable to upload lifecycle policy", e)
                    .Write();
            }
        }

        string GetMyActorVersion() => typeof(LifecyclePolicyUploader).Assembly.GetName().Version?.ToString();


        async Task UploadPolicyAsync(string policyId, LifecyclePolicy policy, CancellationToken cancellationToken)
        {
            IPutLifecycleRequest r = new PutLifecycleRequest(policyId)
            {
                Policy = policy.Policy
            };
            await _esTools.LifecyclePolicy(policyId).PutAsync(r, cancellationToken);
        }
    }
}
