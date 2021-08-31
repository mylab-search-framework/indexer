using LinqToDB.Mapping;

namespace FunctionTests
{
    [Table("test")]
    public class TestEntity
    {
        [Column]
        public int Id { get; set; }
        [Column]
        public string Value { get; set; }

        [Column]
        public bool Bool { get; set; }
    }
}