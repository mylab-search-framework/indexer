using System;
using System.Collections.Generic;
using MyLab.Log;
using MyLab.Search.Indexer.DataContract;

namespace MyLab.Search.Indexer.Tools
{
    class DsEntityPreprocessor
    {
        private readonly JobOptions _jobOptions;

        public DsEntityPreprocessor(JobOptions jobOptions)
        {
            _jobOptions = jobOptions;
        }

        public DataSourceEntity Process(DataSourceEntity entity)
        {
            var newDict = new Dictionary<string, DataSourcePropertyValue>(entity.Properties);

            if (_jobOptions.NewUpdatesStrategy == NewUpdatesStrategy.Update)
            {
                if(_jobOptions.LastChangeProperty == null)
                    throw new InvalidOperationException("LastChangeProperty not defined")
                        .AndFactIs("job-id", _jobOptions.JobId);

                newDict.Remove(_jobOptions.LastChangeProperty);
            }

            return new DataSourceEntity
            {
                Properties = newDict
            };
        }
    }
}
