namespace MyLab.Search.Indexer.Models
{
    public class IndexingRequest
    {
        public string IndexId { get; set; }
        public IndexingRequestEntity[] PostList { get; set; }
        public IndexingRequestEntity[] PutList { get; set; }
        public IndexingRequestEntity[] PatchList { get; set; }
        public string[] DeleteList { get; set; }

        public IndexingRequest Clone()
        {
            return new IndexingRequest
            {
                PostList = PostList,
                PutList = PutList,
                DeleteList = DeleteList,
                PatchList = PatchList,
                IndexId = IndexId
            };
        }
    }
}