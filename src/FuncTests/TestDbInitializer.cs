using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using MyLab.DbTest;

namespace FuncTests
{
    public class TestDbInitializer : ITestDbInitializer
    {
        public async Task InitializeAsync(DataConnection dc)
        {
            await dc.CreateTableAsync<TestDoc>();
        }
    }
}