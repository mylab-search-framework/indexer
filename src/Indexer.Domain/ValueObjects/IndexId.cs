using System.ComponentModel.DataAnnotations;

namespace Indexer.Domain.ValueObjects;

public class IndexId
{
    public string Value { get; protected set; }

    private IndexId(string value)
    {
        if (!Validate(value))
            throw new ValidationException();
        Value = value;
    }

    public static bool TryCreate(string id, out IndexId? result)
    {
        if (!Validate(id))
        {
            result = null;
            return false;
        }

        result = new IndexId(id);
        return true;
    }

    public static bool Validate(string strValue) => !string.IsNullOrWhiteSpace(strValue);
}

