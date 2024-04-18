using MyLab.Search.Indexer.Model.Validators;

namespace IndexerUnitTests.Validators
{
    public class LiteralIdValidatorBehavior
    {
        [Theory]
        [InlineData("foo", true)]
        [InlineData("foo-bar", true)]
        [InlineData("foo-bar-2334", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("   \t", false)]
        public void ShouldValidate(string input, bool expectedValidationResult)
        {
            //Arrange
            var idValidator = new LiteralIdValidator();

            //Act
            var validationResult = idValidator.Validate(input);

            //Assert
            Assert.Equal(expectedValidationResult, validationResult.IsValid);
        }
    }
}