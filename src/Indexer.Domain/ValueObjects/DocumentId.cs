﻿using System.ComponentModel.DataAnnotations;

namespace Indexer.Domain.ValueObjects;

public record DocumentId
{
    public string Value { get; protected set; }

    public DocumentId(string value)
    {
        if (!Validate(value))
            throw new ValidationException();
        Value = value;
    }

    public static bool TryParse(string id, out DocumentId? result)
    {
        if (!Validate(id))
        {
            result = null;
            return false;
        }

        result = new DocumentId(id);
        return true;
    }

    public static bool Validate(string strValue) => !string.IsNullOrWhiteSpace(strValue);
}