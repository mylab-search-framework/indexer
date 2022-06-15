using System;
using LinqToDB.Mapping;
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

        public static TestDoc Generate()
        {
            return Generate(new Random(DateTime.Now.Millisecond).Next());
        }

        public static TestDoc Generate(int id)
        {
            return new TestDoc
            {
                Id = id,
                Content = Guid.NewGuid().ToString("N")
            };
        }
    }
}
