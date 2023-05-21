﻿namespace MyLab.Search.Indexer.Options
{
    public class IndexOptions : IndexOptionsBase
    {
        public string Id { get; set; }
        public string SyncDbQuery { get; set; }
        public int SyncPageSize { get; set; } = 500;
    }
}