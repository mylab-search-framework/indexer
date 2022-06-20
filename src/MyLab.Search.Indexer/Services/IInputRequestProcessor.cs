using System;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Services
{
    public interface IInputRequestProcessor
    {
        Task IndexAsync(InputIndexingRequest inputRequest);
    }

    class KickDocsCountMismatchException : Exception
    {
        public KickDocsCountMismatchException()
        : base("Kicked docs count mismatch")
        {
            
        }
    }
}
