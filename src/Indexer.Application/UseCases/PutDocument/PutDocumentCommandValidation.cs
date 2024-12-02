using FluentValidation;
using FluentValidation.Results;
using Indexer.Domain.ValueObjects;
using System;

namespace Indexer.Application.UseCases.PutDocument;

public static class PutDocumentCommandValidation
{
    private static readonly InlineValidator<PutDocumentCommand> Validator = new()
    {
        v => v.RuleFor(c => c.IndexId).Must((_, val, _) => IndexId.Validate(val)),
        v => v.RuleFor(c => c.Document).NotNull()
    };
    public static ValidationResult Validate(this PutDocumentCommand command)
    {
        return Validator.Validate(command);
    }

    public static void ValidateAndThrow(this PutDocumentCommand command)
    {
        Validator.ValidateAndThrow(command);
    }
}
