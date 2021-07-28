using System.Collections.Generic;

namespace MyLab.Search.Indexer
{
    public class DataSourceEntity
    {
        //[DynamicColumnsStore]
        public IDictionary<string, string> Properties { get; set; }


        //public static void RegisterMapping()
        //{
        //    var mb = MappingSchema.Default.GetFluentMappingBuilder();

        //    mb.Entity<DataSourceEntity>() //.HasTableName("main")
        //        .DynamicColumnsStore(e => e.Properties);
        //    //.Property(x => Sql.Property<int>(x, "Id"))
        //    //.Property(x => Sql.Property<string>(x, "Value"));
        //}
    }
}