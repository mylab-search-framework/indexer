using FluentValidation;
using FluentValidation.Results;
using Indexer.Domain.ValueObjects;

namespace Indexer.Application.UseCases.PatchDocument;

public static class PatchDocumentCommandValidation
{
    private static readonly InlineValidator<PatchDocumentCommand> Validator = new()
    {
        v => v.RuleFor(c => c.IndexId).Must((_, val, _) => IndexId.Validate(val)),
        v => v.RuleFor(c => c.DocumentPart).NotNull()
    };
    public static ValidationResult Validate(this PatchDocumentCommand command)
    {
        return Validator.Validate(command);
    }

    public static void ValidateAndThrow(this PatchDocumentCommand command)
    {
        Validator.ValidateAndThrow(command);
    }
}
