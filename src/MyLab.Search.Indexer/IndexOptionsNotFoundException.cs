namespace MyLab.Search.Indexer;

class IndexOptionsNotFoundException : Exception
{
    public IndexOptionsNotFoundException()
     : base("Index not found")    
    {
        
    }
}