using System;
using System.Collections.Generic;
using MyLab.Log;
using MyLab.Search.Indexer.DataContract;

namespace MyLab.Search.Indexer.Tools
{
    class DsEntityPreprocessor
    {
        private readonly NsOptions _nsOptions;

        public DsEntityPreprocessor(NsOptions nsOptions)
        {
            _nsOptions = nsOptions;
        }

        public DataSourceEntity Process(DataSourceEntity entity)
        {
            var newDict = new Dictionary<string, DataSourcePropertyValue>(entity.Properties);

            if (_nsOptions.NewUpdatesStrategy == NewUpdatesStrategy.Update)
            {
                if(_nsOptions.LastChangeProperty == null)
                    throw new InvalidOperationException("LastChangeProperty not defined")
                        .AndFactIs("job-id", _nsOptions.NsId);

                newDict.Remove(_nsOptions.LastChangeProperty);
            }

            return new DataSourceEntity
            {
                Properties = newDict
            };
        }
    }
}
