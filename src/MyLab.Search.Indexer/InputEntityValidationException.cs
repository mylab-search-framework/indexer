using System;

namespace MyLab.Search.Indexer
{
    public class InputEntityValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="InputEntityValidationException"/>
        /// </summary>
        public InputEntityValidationException(string message) : base(message)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of <see cref="InputEntityValidationException"/>
        /// </summary>
        public InputEntityValidationException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
