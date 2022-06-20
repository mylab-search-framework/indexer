using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using MyLab.DbTest;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Tools;
using Nest;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests
{
    public class DataSourcePropertyTypeConverterBehavior : IClassFixture<TmpDbFixture>
    {
        private readonly TmpDbFixture _fxt;
        private readonly ITestOutputHelper _output;

        public DataSourcePropertyTypeConverterBehavior(TmpDbFixture fxt, ITestOutputHelper output)
        {
            fxt.Output = output;
            _fxt = fxt;
            _output = output;
        }

        [Fact]
        public async Task ShouldConvertPropertyWrite()
        {
            //Arrange
            var dt = DateTime.Now;
            var entity = new TestEntity
            {
                Boolean = true,

                Date = dt.Date,
                DateTime = dt.AddSeconds(1),
                DateTime2 = dt.AddSeconds(2),
                DateTimeOffset = dt,

                Int16 = 104,
                Int32 = 105,
                Int64 = 100,

                Single = 101.1,
                Decimal = 102.1M,
                Double = 103.3d,

                Char = "bar",
                Varchar = "foo",
                NVarchar = "baz",

                Guid = Guid.NewGuid()
            };

            var dbMgr = await _fxt.CreateDbAsync(new TestDbInitializer(entity));

            const string expectedDtFormat = "yyyy-MM-dd HH:mm:ss.fffffff";
            var cult = CultureInfo.InvariantCulture;

            //Act
            await using var c=  dbMgr.Use();

            var found = c.Query(reader =>
            {
                var props = new Dictionary<string, DataSourcePropertyValue>();

                for (var index = 0; index < reader.FieldCount; index++)
                {
                    var fieldName = reader.GetName(index);
                    var fieldValue = reader.GetString(index);
                    var fieldNetType = reader.GetFieldType(index);
                    var fieldStrType = reader.GetDataTypeName(index);
                    var convertedType = DataSourcePropertyTypeConverter.Convert(fieldStrType);


                    props.Add(fieldName,
                        new DataSourcePropertyValue
                        {
                            Value = fieldValue,
                            DbType = convertedType
                        });

                    _output.WriteLine($"{index} ({fieldName}|{fieldStrType}): '{fieldNetType.Name}' -> '{convertedType}' = '{fieldValue}'");
                }

                return props;
            }, "select * from test_entity")
                .First();

            //Assert
            Assert.Equal(DataSourcePropertyType.Boolean, found[nameof(TestEntity.Boolean)].DbType);
            Assert.Equal(DataSourcePropertyType.DateTime, found[nameof(TestEntity.Date)].DbType);
            Assert.Equal(DataSourcePropertyType.DateTime, found[nameof(TestEntity.DateTime)].DbType);
            Assert.Equal(DataSourcePropertyType.DateTime, found[nameof(TestEntity.DateTime2)].DbType);
            Assert.Equal(DataSourcePropertyType.DateTime, found[nameof(TestEntity.DateTimeOffset)].DbType);
            Assert.Equal(DataSourcePropertyType.Numeric, found[nameof(TestEntity.Int16)].DbType);
            Assert.Equal(DataSourcePropertyType.Numeric, found[nameof(TestEntity.Int32)].DbType);
            Assert.Equal(DataSourcePropertyType.Numeric, found[nameof(TestEntity.Int64)].DbType);
            Assert.Equal(DataSourcePropertyType.Double, found[nameof(TestEntity.Single)].DbType);
            Assert.Equal(DataSourcePropertyType.Double, found[nameof(TestEntity.Decimal)].DbType);
            Assert.Equal(DataSourcePropertyType.Double, found[nameof(TestEntity.Double)].DbType);
            Assert.Equal(DataSourcePropertyType.String, found[nameof(TestEntity.Char)].DbType);
            Assert.Equal(DataSourcePropertyType.String, found[nameof(TestEntity.Varchar)].DbType);
            Assert.Equal(DataSourcePropertyType.String, found[nameof(TestEntity.NVarchar)].DbType);
            Assert.Equal(DataSourcePropertyType.Undefined, found[nameof(TestEntity.Guid)].DbType);

            Assert.Equal((entity.Boolean ? 1 : 0).ToString(), found[nameof(TestEntity.Boolean)].Value);
            Assert.Equal(entity.Date, DateTime.Parse(found[nameof(TestEntity.Date)].Value));
            Assert.Equal(entity.DateTime, DateTime.Parse(found[nameof(TestEntity.DateTime)].Value));
            Assert.Equal(entity.DateTime2, DateTime.Parse(found[nameof(TestEntity.DateTime2)].Value));
            Assert.Equal(entity.DateTimeOffset, DateTime.Parse(found[nameof(TestEntity.DateTimeOffset)].Value));
            Assert.Equal(entity.Int16.ToString(), found[nameof(TestEntity.Int16)].Value);
            Assert.Equal(entity.Int32.ToString(), found[nameof(TestEntity.Int32)].Value);
            Assert.Equal(entity.Int64.ToString(), found[nameof(TestEntity.Int64)].Value);
            Assert.Equal(entity.Single, double.Parse(found[nameof(TestEntity.Single)].Value, cult));
            Assert.Equal(entity.Decimal, decimal.Parse(found[nameof(TestEntity.Decimal)].Value, cult));
            Assert.Equal(entity.Double, double.Parse(found[nameof(TestEntity.Double)].Value, cult));
            Assert.Equal(entity.Char, found[nameof(TestEntity.Char)].Value);
            Assert.Equal(entity.Varchar, found[nameof(TestEntity.Varchar)].Value);
            Assert.Equal(entity.NVarchar, found[nameof(TestEntity.NVarchar)].Value);
        }
        
        [Table("test_entity")]
        class TestEntity
        {
            [Column(DataType = DataType.Int16)]
            public short Int16 { get; set; }
            [Column(DataType = DataType.Int32)]
            public int Int32 { get; set; }
            [Column(DataType = DataType.Int64)]
            public long Int64 { get; set; }
            [Column(DataType = DataType.Single)]
            public double Single { get; set; }
            [Column(DataType = DataType.Date)]
            public DateTime Date { get; set; }
            [Column(DataType = DataType.DateTime)]
            public DateTime DateTime { get; set; }
            [Column(DataType = DataType.DateTime2)]
            public DateTime DateTime2 { get; set; }
            [Column(DataType = DataType.DateTimeOffset)]
            public DateTime DateTimeOffset { get; set; }
            [Column(DataType = DataType.Boolean)]
            public bool Boolean { get; set; }
            [Column(DataType = DataType.Decimal)]
            public decimal Decimal { get; set; }
            [Column(DataType = DataType.Double)]
            public double Double { get; set; }
            [Column(DataType = DataType.NVarChar)]
            public string NVarchar { get; set; }
            [Column(DataType = DataType.VarChar)]
            public string Varchar { get; set; }
            [Column(DataType = DataType.Char, Length = 100)]
            public string Char { get; set; }
            [Column(DataType = DataType.Guid)]
            public Guid Guid{ get; set; }
        }

        class TestDbInitializer : ITestDbInitializer
        {
            private readonly TestEntity _initEntity;

            public TestDbInitializer(TestEntity initEntity)
            {
                _initEntity = initEntity;
            }

            public async Task InitializeAsync(DataConnection dataConnection)
            {
                await dataConnection.CreateTableAsync<TestEntity>();

                await dataConnection.InsertAsync(_initEntity);
            }
        }
    }
}
