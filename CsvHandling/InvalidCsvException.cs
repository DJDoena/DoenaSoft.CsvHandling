using System;

namespace DoenaSoft.CsvHandling
{
    /// <summary />
    public sealed class InvalidCsvException : Exception
    {
        /// <summary />
        public InvalidCsvException(string message) : base(message)
        {
        }
    }
}