using System;
using LinqToDB.Mapping;
using Nest;
using Newtonsoft.Json;

namespace FuncTests
{
    [Table("test_doc")]
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class TestDoc
    {
        [Column("id")]
        [Keyword(Name = "id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [Text(Name = "content")]
        [JsonProperty("content")]
        [Column("content")]
        public string Content { get; set; }
        
        [Column("changed")]
        public DateTime? LastChanged { get; set; }

        public static TestDoc Generate()
        {
            return Generate(Guid.NewGuid().ToString("N"));
        }

        public static TestDoc Generate(string id)
        {
            return new TestDoc
            {
                Id = id,
                Content = Guid.NewGuid().ToString("N")
            };
        }
    }
}
