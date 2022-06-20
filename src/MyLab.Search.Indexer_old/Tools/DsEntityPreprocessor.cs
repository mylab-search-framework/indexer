using System;
using System.Collections.Generic;
using MyLab.Log;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Tools
{
    class DsEntityPreprocessor
    {
        private readonly IdxOptions _idxOptions;

        public DsEntityPreprocessor(IdxOptions idxOptions)
        {
            _idxOptions = idxOptions;
        }

        public DataSourceEntity Process(DataSourceEntity entity)
        {
            var newDict = new Dictionary<string, DataSourcePropertyValue>(entity.Properties);

            if (_idxOptions.NewUpdatesStrategy == NewUpdatesStrategy.Update)
            {
                if(_idxOptions.LastChangeProperty == null)
                    throw new InvalidOperationException("LastChangeProperty not defined")
                        .AndFactIs("index", _idxOptions.Id);

                newDict.Remove(_idxOptions.LastChangeProperty);
            }

            return new DataSourceEntity
            {
                Properties = newDict
            };
        }
    }
}
