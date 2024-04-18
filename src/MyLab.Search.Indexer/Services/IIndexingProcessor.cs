using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Services
{
    interface IIndexingProcessor
    {
        public Task ProcessAsync(IndexInstructions instructions);
    }
}
