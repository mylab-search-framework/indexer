using System.Threading.Tasks;

namespace MyLab.Search.Indexer.LogicStrategy
{
    interface ISeedCalc
    {
        Task StartAsync();

        void Update(DataSourceEntity[] entities);

        Task SaveAsync();

        string GetLogValue();
    }
}