namespace MyLab.Search.Indexer.Options
{
    public class IdxOptions
    {
        public string Id { get; set; }
        public string MqQueue { get; set; }
        public NewUpdatesStrategy NewUpdatesStrategy { get; set; }
        public NewIndexStrategy NewIndexStrategy { get; set; }
        public string LastChangeProperty { get; set; }
        public string IdPropertyName { get; set; }
        public IdPropertyType IdPropertyType { get; set; }
        public int PageSize { get; set; }
        public bool EnablePaging { get; set; } = false;
        public string SyncDbQuery { get; set; }
        public string KickDbQuery { get; set; }
        public string EsIndex { get; set; }
        
        public IdxOptions()
        {
            
        }
        
        public IdxOptions(NsOptions nsOptions)
        {
            Id = nsOptions.NsId;
            MqQueue = nsOptions.NsId;
            NewUpdatesStrategy = nsOptions.NewUpdatesStrategy;
            NewIndexStrategy = nsOptions.NewIndexStrategy;
            LastChangeProperty = nsOptions.LastChangeProperty;
            IdPropertyName = nsOptions.IdPropertyName;
            IdPropertyType = nsOptions.IdPropertyType;
            PageSize = nsOptions.PageSize;
            EnablePaging = nsOptions.EnablePaging;
            SyncDbQuery = nsOptions.SyncDbQuery;
            KickDbQuery = nsOptions.KickDbQuery;
            EsIndex = nsOptions.EsIndex;
        }
    }
}