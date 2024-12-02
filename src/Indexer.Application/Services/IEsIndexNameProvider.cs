using Indexer.Application.Options;
using Indexer.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Indexer.Application.Services;

public interface IEsIndexNameProvider
{
    string Provide(IndexId idxId);
}

class EsIndexNameProvider : IEsIndexNameProvider
{
    private readonly IndexerAppOptions _opts;

    public EsIndexNameProvider(IOptions<IndexerAppOptions> opts)
    : this(opts.Value)
    {
    }

    public EsIndexNameProvider(IndexerAppOptions opts)
    {
        _opts = opts;
    }

    public string Provide(IndexId idxId)
    {
        string baseName = idxId.Value;
        
        if (_opts.IndexMap != null && _opts.IndexMap.TryGetValue(idxId.Value, out var foundTranslate))
            baseName = foundTranslate;

        return $"{_opts.IndexPrefix ?? string.Empty}{baseName}{_opts.IndexSuffix ?? string.Empty}";
    }
}