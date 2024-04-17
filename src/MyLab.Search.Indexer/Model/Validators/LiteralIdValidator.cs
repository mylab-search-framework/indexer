using FluentValidation;

namespace MyLab.Search.Indexer.Model.Validators
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
