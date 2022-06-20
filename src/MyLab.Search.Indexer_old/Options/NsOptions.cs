using System;

namespace MyLab.Search.Indexer.Options
{
    [Obsolete]
    public class NsOptions
    {
        public string NsId { get; set; }
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
    }
}