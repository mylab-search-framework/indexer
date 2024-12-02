using FluentValidation;
using FluentValidation.Results;
using Indexer.Application.UseCases.PatchDocument;
using Indexer.Domain.ValueObjects;

namespace Indexer.Application.UseCases.DeleteDocument;

public static class DeleteDocumentCommandValidation
{
    private static readonly InlineValidator<DeleteDocumentCommand> Validator = new()
    {
        v => v.RuleFor(c => c.IndexId).Must((_, val, _) => IndexId.Validate(val)),
        v => v.RuleFor(c => c.IndexId).Must((_, val, _) => DocumentId.Validate(val))
    };
    public static ValidationResult Validate(this DeleteDocumentCommand command)
    {
        return Validator.Validate(command);
    }

    public static void ValidateAndThrow(this DeleteDocumentCommand command)
    {
        Validator.ValidateAndThrow(command);
    }
}
