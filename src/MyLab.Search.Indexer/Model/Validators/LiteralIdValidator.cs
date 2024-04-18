using FluentValidation;
using FluentValidation.Results;

namespace MyLab.Search.Indexer.Model.Validators
{
    public class LiteralIdValidator : AbstractValidator<LiteralId?>
    {
        public LiteralIdValidator()
        {
            RuleFor(id => id!.Value)
                .NotNull()
                .NotEmpty()
                .Must(id => id?.All(ch => !char.IsWhiteSpace(ch)) ?? true).WithMessage("A value must not have spaces");
        }

        public override ValidationResult Validate(ValidationContext<LiteralId?> context)
        {
            if (context.InstanceToValidate == null)
                return new ValidationResult(new[]
                {
                    new ValidationFailure("Id", "Identifier cannot be null")
                });
            return base.Validate(context);
        }
    }
}
