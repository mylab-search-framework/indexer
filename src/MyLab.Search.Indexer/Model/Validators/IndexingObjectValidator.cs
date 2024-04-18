using FluentValidation;

namespace MyLab.Search.Indexer.Model.Validators
{
    class IndexingObjectValidator : AbstractValidator<IndexingObject>
    {
        public IndexingObjectValidator()
        {
            RuleFor(o => o.Id)
                .NotNull()
                .SetValidator(new LiteralIdValidator());
            RuleFor(o => o.Value)
                .NotNull()
                .Must(j => j.Count != 0);
        }
    }
}
