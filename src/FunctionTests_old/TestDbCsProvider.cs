using MyLab.Db;

namespace FunctionTests
{
    class TestDbCsProvider : IConnectionStringProvider
    {
        public string GetConnectionString(string name = null)
        {
            return "server=localhost;uid=user;pwd=pass;database=test";
        }
    }
}