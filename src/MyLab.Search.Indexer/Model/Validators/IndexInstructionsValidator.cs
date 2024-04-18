using FluentValidation;

namespace MyLab.Search.Indexer.Model.Validators
{
    public class IndexInstructionsValidator : AbstractValidator<IndexInstructions>
    {
        public IndexInstructionsValidator()
        {
            RuleFor(i => i.IndexId)
                .NotNull()
                .SetValidator(new LiteralIdValidator());
            RuleFor(i => i).Must
            (
                (instr, _) =>
                    instr.DeleteList is { Count: > 0 } || 
                    instr.PatchList is { Count: > 0 } ||
                    instr.PutList is { Count: > 0 }
            ).WithMessage("No index instructions");
        }
    }
}
