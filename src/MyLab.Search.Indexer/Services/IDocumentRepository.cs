using System.Text.Json.Nodes;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Services
{
    interface IDocumentRepository
    {
        Task<IReadOnlyList<IndexingObject>> GetDocumentsAsync(LiteralId[] ids);
    }
}
