using System;
using LinqToDB;
using LinqToDB.DataProvider.MySql;
using MyLab.DbTest;

namespace FunctionTests
{
    public class IndexerTestDbFixture : RemoteDbFixture, IDisposable
    {
        public IndexerTestDbFixture() 
            : base(new MySqlDataProvider(ProviderName.MySql), "server=localhost;uid=user;pwd=pass;database=test")
        {
            using var c = Manager.Use();

            c.DropTable<TestEntity>(throwExceptionIfNotExists:false);
            c.CreateTable<TestEntity>();
        }

        public void Dispose()
        {
            Manager.DoOnce().DropTable<TestEntity>(throwExceptionIfNotExists:false);
        }
    }
}