namespace MyLab.Search.Indexer.Model
{
    public class LiteralId
    {
        public string? Value { get; }

        public LiteralId(string? value)
        {
            Value = value;
        }

        public override string? ToString()
        {
            return Value;
        }

        public static implicit operator LiteralId(string? value)
        {
            return new LiteralId(value);
        }

        public static implicit operator string?(LiteralId? value)
        {
            return value?.ToString();
        }
    }
}
