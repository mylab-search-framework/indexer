using System.Threading.Tasks;
using LinqToDB.Data;

namespace MyLab.Search.Indexer.LogicStrategy
{
    interface IIndexerLogicStrategy
    {
        ISeedCalc CreateSeedCalc();

        Task<DataParameter> CreateSeedDataParameterAsync();
    }
}