using System;

namespace MyLab.Search.Indexer
{
    public class BadIndexingRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BadIndexingRequestException"/>
        /// </summary>
        public BadIndexingRequestException(string message) : base(message)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BadIndexingRequestException"/>
        /// </summary>
        public BadIndexingRequestException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
