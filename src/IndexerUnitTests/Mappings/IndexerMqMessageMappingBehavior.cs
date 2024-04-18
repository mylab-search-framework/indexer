using System.Text.Json.Nodes;
using AutoMapper;
using MyLab.Search.Indexer.Model;
using MyLab.Search.Indexer.MqConsuming;

namespace IndexerUnitTests.Mappings
{
    public class IndexerMqMessageMappingBehavior
    {
        private readonly IMapper _mapper;
        
        public IndexerMqMessageMappingBehavior()
        {
            var mappingConfig = new MapperConfiguration(ce => ce.AddProfile<IndexerMqMessageMappingProfile>());
            _mapper = new Mapper(mappingConfig);
        }

        [Fact]
        public void ShouldMapIntoIndexInstructions()
        {
            //Arrange
            var mqMsg = new IndexerMqMessage
            {
                IndexId = "foo",
                PutList = new[]
                {
                    TestTools.CreateEmptyIndexingObject("bar")
                },
                PatchList = new[]
                {
                    TestTools.CreateEmptyIndexingObject("baz")
                },
                DeleteList = new[]
                {
                    new LiteralId("qoz")
                }
            };

            //Act
            var idxInstructions = _mapper.Map<IndexerMqMessage, IndexInstructions>(mqMsg);

            //Assert
            Assert.Equal("foo", idxInstructions.IndexId);
            Assert.NotNull(idxInstructions.PutList);
            Assert.NotNull(idxInstructions.PatchList);
            Assert.NotNull(idxInstructions.DeleteList);
            Assert.Contains(idxInstructions.PutList, d => d.Id == "bar");
            Assert.Contains(idxInstructions.PatchList, d => d.Id == "baz");
            Assert.Contains(idxInstructions.DeleteList, id => id == "qoz");
        }
    }
}
