using LinqToDB.Reflection;
using MyLab.Search.EsAdapter;

namespace MyLab.Search.Indexer.Tools
{
    class MqCaseOptionsValidator
    {
        private readonly IndexerOptions _options;
        private readonly ElasticsearchOptions _esOptions;

        public MqCaseOptionsValidator(IndexerOptions options, ElasticsearchOptions esOptions)
        {
            _options = options;
            _esOptions = esOptions;
        }

        public void Validate()
        {
            OptionsValidatorTools.CheckId(_options);
            OptionsValidatorTools.CheckEs(_esOptions);
        }
    }

    class DbCaseOptionsValidator
    {
        private readonly IndexerOptions _options;
        private readonly IndexerDbOptions _dbOptions;
        private readonly ElasticsearchOptions _esOptions;

        public DbCaseOptionsValidator(IndexerOptions options, IndexerDbOptions dbOptions, ElasticsearchOptions esOptions)
        {
            _options = options;
            _dbOptions = dbOptions;
            _esOptions = esOptions;
        }

        public void Validate()
        {
            OptionsValidatorTools.CheckId(_options);
            OptionsValidatorTools.CheckEs(_esOptions);

            OptionsValidatorTools.ThrowNotDefined(_dbOptions, o => o.Strategy);
            OptionsValidatorTools.ThrowNotDefined(_dbOptions, o => o.Query);
            OptionsValidatorTools.ThrowNotDefined(_dbOptions, o => o.Provider);

            if(_dbOptions.Strategy == IndexerDbStrategy.Update)
                OptionsValidatorTools.ThrowNotDefined(_options, o => o.LastModifiedFieldName);

            if (_dbOptions.EnablePaging)
                OptionsValidatorTools.ThrowNotDefined(_dbOptions, o => o.PageSize);

        }
    }
}
