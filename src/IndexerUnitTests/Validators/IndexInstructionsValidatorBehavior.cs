using MyLab.Search.Indexer.Model;
using MyLab.Search.Indexer.Model.Validators;

namespace IndexerUnitTests.Validators
{
    public class IndexInstructionsValidatorBehavior
    {
        [Theory]
        [InlineData("foo", true)]
        [InlineData(null, false)]
        public void ShouldFailIfInvalidIndexId(string id, bool success)
        {
            //Arrange
            var instructions = new IndexInstructions
            {
                IndexId = id,
                DeleteList = new [] { new LiteralId("bar") }
            };

            var validator = new IndexInstructionsValidator();

            //Act
            var result = validator.Validate(instructions);

            //Assert
            Assert.Equal(success, result.IsValid);
        }

        [Fact]
        public void ShouldPassIfAllGood()
        {
            //Arrange
            var instructions = new IndexInstructions
            {
                IndexId = "foo",
                DeleteList = new[] { new LiteralId("bar") }
            };

            var validator = new IndexInstructionsValidator();

            //Act
            var result = validator.Validate(instructions);

            //Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ShouldFailWhenAllListsIsEmpty()
        {
            //Arrange
            var instructions = new IndexInstructions
            {
                IndexId = "foo",
                DeleteList = Array.Empty<LiteralId>()
            };

            var validator = new IndexInstructionsValidator();

            //Act
            var result = validator.Validate(instructions);

            //Assert
            Assert.False(result.IsValid);
        }
    }
}
