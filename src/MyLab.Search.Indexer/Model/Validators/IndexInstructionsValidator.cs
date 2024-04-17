using FluentValidation;

namespace MyLab.Search.Indexer.Model.Validators
{
    public class IndexInstructionsValidator : AbstractValidator<IndexInstructions>
    {
        public IndexInstructionsValidator()
        {
            RuleFor(i => i.IndexId).SetValidator(new LiteralIdValidator());
            RuleFor(i => i).Must
            (
                (instr, _) =>
                    instr.DeleteList is { Length: > 0 } || 
                    instr.PatchList is { Length: > 0 } ||
                    instr.PutList is { Length: > 0 }
            ).WithMessage("No index instructions");
        }
    }
}
