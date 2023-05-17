using System;
using System.Collections.Generic;
using MyLab.Search.Indexer.Tools;
using Xunit;

namespace UnitTests
{
    public class DictionarySerializerBehavior
    {
        [Fact]
        public void ShouldSerializeObject()
        {
            //Arrange
            var testObj = new TestObj
            {
                DateTimeProperty = new DateTime(2000, 1, 2),
                GuidProperty = Guid.NewGuid(),
                IntProperty = 10,
                ObjProperty = new TestObj
                {
                    StringProperty = "foo"
                },
                StringProperty = "bar",
                PropWithAttrProperty = "baz"
            };

            var dict = new Dictionary<string, object>();

            //Act
            DictionarySerializer.Serialize(dict, testObj);

            dict.TryGetValue("ObjProperty", out var objPropVal);
            dict.TryGetValue("StringProperty", out var stringVal);
            dict.TryGetValue("DateTimeProperty", out var dateTimeVal);
            dict.TryGetValue("GuidProperty", out var guidVal);
            dict.TryGetValue("IntProperty", out var intVal);
            dict.TryGetValue("prop-with-attr", out var withAttrVal);

            var serializedNestedObj = objPropVal as IDictionary<string, object>;

            //Arrange
            Assert.Equal("bar", stringVal as string);
            Assert.Equal("10", intVal as string);
            Assert.Equal(testObj.GuidProperty.ToString("N"), guidVal as string);
            Assert.Equal("2000-01-02T00:00:00.0000000", dateTimeVal as string);
            Assert.Equal("baz", withAttrVal as string);
            Assert.NotNull(serializedNestedObj);
            Assert.Contains(serializedNestedObj, d => d.Key == "StringProperty" && (string)d.Value == "foo");
        }

        [Fact]
        public void ShouldDeserializeObject()
        {
            //Arrange 
            var guid = Guid.NewGuid();
            var datetime = DateTime.Now;

            var dict = new Dictionary<string, object>
            {
                { "StringProperty", "foo" },
                { "IntProperty", "10" },
                { "GuidProperty", guid.ToString("N") },
                { "DateTimeProperty", datetime.ToString("O") },
                { "prop-with-attr", "bar" },
                { "extra", "extra" },
                {
                    "ObjProperty", new Dictionary<string, object>
                    {
                        { "StringProperty", "baz" }
                    }
                }

            };

            //Act
            var obj = DictionarySerializer.Deserialize<TestObj>(dict);

            //Assert
            Assert.NotNull(obj);
            Assert.Equal("foo", obj.StringProperty);
            Assert.Equal(10, obj.IntProperty);
            Assert.Equal(guid, obj.GuidProperty);
            Assert.Equal(datetime, obj.DateTimeProperty);
            Assert.Equal("bar", obj.PropWithAttrProperty);
            Assert.NotNull(obj.ObjProperty);
            Assert.Equal("baz", obj.ObjProperty.StringProperty);
            Assert.Null(obj.ObjProperty.PropWithAttrProperty);
            Assert.Null(obj.ObjProperty.ObjProperty);
            Assert.Equal(default,obj.ObjProperty.DateTimeProperty);
            Assert.Equal(default,obj.ObjProperty.IntProperty);
            Assert.Equal(default,obj.ObjProperty.GuidProperty);
        }

        [Fact]
        public void ShouldInitializeObject()
        {
            //Arrange 
            var guid = Guid.NewGuid();
            var datetime = DateTime.Now;

            var dict = new Dictionary<string, object>
            {
                { "StringProperty", "foo" },
                { "IntProperty", "10" },
                { "GuidProperty", guid.ToString("N") },
                { "DateTimeProperty", datetime.ToString("O") },
                { "prop-with-attr", "bar" },
                { "extra", "extra" },
                {
                    "ObjProperty", new Dictionary<string, object>
                    {
                        { "StringProperty", "baz" }
                    }
                }

            };

            var obj = new TestObj();

            //Act
            DictionarySerializer.Initialize(obj, dict);

            //Assert
            Assert.NotNull(obj);
            Assert.Equal("foo", obj.StringProperty);
            Assert.Equal(10, obj.IntProperty);
            Assert.Equal(guid, obj.GuidProperty);
            Assert.Equal(datetime, obj.DateTimeProperty);
            Assert.Equal("bar", obj.PropWithAttrProperty);
            Assert.NotNull(obj.ObjProperty);
            Assert.Equal("baz", obj.ObjProperty.StringProperty);
            Assert.Null(obj.ObjProperty.PropWithAttrProperty);
            Assert.Null(obj.ObjProperty.ObjProperty);
            Assert.Equal(default, obj.ObjProperty.DateTimeProperty);
            Assert.Equal(default, obj.ObjProperty.IntProperty);
            Assert.Equal(default, obj.ObjProperty.GuidProperty);
        }

        [Fact]
        public void ShouldInitializeInitiableObject()
        {
            //Arrange 
            var guid = Guid.NewGuid().ToString("N");

            var dict = new Dictionary<string, object>
            {
                { "foo", guid }
            };

            //Act
            var obj = DictionarySerializer.Deserialize<TestInitialbeObj>(dict);

            //Assert
            Assert.NotNull(obj);
            Assert.Equal(guid, obj.Foo);
        }

        class TestObj
        {
            [DictProperty("prop-with-attr")]
            public string PropWithAttrProperty { get; set; }
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public Guid GuidProperty { get; set; }
            public DateTime DateTimeProperty{ get; set; }
            public TestObj ObjProperty { get; set; }
        }

        class TestInitialbeObj : IDictionaryInitiable
        {
            public string Foo { get; set; }
            public void Initialize(IDictionary<string, object> dict)
            {
                if (dict.TryGetValue("foo", out var fooVal))
                    Foo = fooVal.ToString();
            }
        }
    }
}