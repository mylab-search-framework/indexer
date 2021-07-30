namespace MyLab.Search.Indexer.LogicStrategy
{
    interface IIndexerLogicStrategy
    {
        ISeedCalc CreateSeedCalc();
    }
}