using System;
using LinqToDB.Mapping;
using Nest;
using Newtonsoft.Json;

namespace IntegrationTests
{
    [Table("test_doc")]
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class TestDoc
    {
        [Column("id")]
        [Keyword(Name = "id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [Column("content")]
        [Text(Name = "id")]
        [JsonProperty("content")]
        public string Content { get; set; }

        [Column("changed")]
        public DateTime? LastChanged { get; set; }

        public TestDoc()
        {
            
        }

        public TestDoc(string id, string content)
        {
            Id = id;
            Content = content;
        }
        
        public static TestDoc Generate()
        {
            return new TestDoc
            {
                Id = Guid.NewGuid().ToString("N"),
                Content = Guid.NewGuid().ToString("N")
            };
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            return Equals((TestDoc)obj);
        }

        protected bool Equals(TestDoc other)
        {
            return Id == other.Id && Content == other.Content;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Content);
        }
    }
}