using FluentValidation;
using LiteralId = MyLab.Search.Indexer.Model.LiteralId;

namespace MyLab.Search.Indexer.Validators
{
    public class LiteralIdValidator : AbstractValidator<LiteralId?>
    {
        public LiteralIdValidator()
        {
            RuleFor(id => id)
                .NotNull();
            RuleFor(id => id!.Value)
                .NotNull()
                .NotEmpty()
                .Must(id => id?.All(ch => !char.IsWhiteSpace(ch)) ?? true).WithMessage("A value must not have spaces");
        }
    }
}
