using System.ComponentModel.DataAnnotations;

namespace MyLab.Search.Indexer.Domain.ValueObjects;

public record DocumentId
{
    public string Value { get; protected set; }

    private DocumentId(string value)
    {
        if (!Validate(value))
            throw new ValidationException();
        Value = value;
    }

    public static bool TryCreate(string id, out DocumentId? result)
    {
        if (!Validate(id))
        {
            result = null;
            return false;
        }

        result = new DocumentId(id);
        return true;
    }

    static bool Validate(string strValue) => !string.IsNullOrWhiteSpace(strValue);
}