using FluentValidation;

namespace MyLab.Search.Indexer.Model.Validators
{
    public class IndexingObjectValidator : AbstractValidator<IndexingObject>
    {
        public IndexingObjectValidator()
        {
            RuleFor(o => o.Id).SetValidator(new LiteralIdValidator());
            RuleFor(o => o.Value)
                .NotNull()
                .Must(j => j.Count != 0);
        }
    }
}
