using System.Collections.Generic;

namespace MyLab.Search.Indexer.Services
{
    public class IndexEntity : Dictionary<string, object>
    {
        public IndexEntity()
        {
            
        }

        public IndexEntity(IDictionary<string, object> initial) : base(initial)
        {
            
        }
    }
}