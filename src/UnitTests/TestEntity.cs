using System;
using Newtonsoft.Json;

namespace UnitTests
{
    class TestDoc
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Content { get; set; }

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

    }
}