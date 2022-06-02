using System;
using Newtonsoft.Json;

namespace UnitTests
{
    class TestEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Content { get; set; }

        public TestEntity()
        {
            
        }

        public TestEntity(string id, string content)
        {
            Id = id;
            Content = content;
        }
        
        public static TestEntity Generate()
        {
            return new TestEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                Content = Guid.NewGuid().ToString("N")
            };
        }

    }
}