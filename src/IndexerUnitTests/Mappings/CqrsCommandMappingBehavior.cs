using AutoMapper;
using MyLab.Search.Indexer.Handlers;
using MyLab.Search.Indexer.Handlers.IndexingRequest;
using MyLab.Search.Indexer.Model;

namespace IndexerUnitTests.Mappings
{
    public  class CqrsCommandMappingBehavior
    {
        private readonly IMapper _mapper;

        public CqrsCommandMappingBehavior()
        {
            var mappingConfig = new MapperConfiguration(ce => ce.AddProfile<CqrsCommandMappingProfile>());
            _mapper = new Mapper(mappingConfig);
        }

        [Fact]
        public void ShouldMapIndexingRequestCommandIntoIndexInstructions()
        {
            //Arrange
            var cmd = new IndexingRequestCommand
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
                },
                KickList = new []
                {
                    new LiteralId("qozz")
                }
            };

            //Act
            var idxInstructions = _mapper.Map<IndexingRequestCommand, IndexInstructions>(cmd);

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
