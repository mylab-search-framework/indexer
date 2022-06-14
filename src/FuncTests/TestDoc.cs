using System;
using LinqToDB.Mapping;
using MyLab.Db;
using Nest;

namespace FuncTests
{
    [Table("test_doc")]
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class TestDoc
    {
        [Column("id")]
        [Number]
        public int Id { get; set; }

        [Keyword]
        [Column("content")]
        public string Content { get; set; }

        [Date(Index = false)]
        [Column("changed")]
        public DateTime? LastChanged { get; set; }
    }
}
