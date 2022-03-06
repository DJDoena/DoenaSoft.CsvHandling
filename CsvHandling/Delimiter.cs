using System.Collections.Generic;

namespace DoenaSoft.CsvHandling
{
    /// <summary>
    /// Returns a list of valid field delimiters
    /// </summary>
    public static class Delimiter
    {
        /// <summary>
        /// Returns a list of valid field delimiters
        /// </summary>
        public static IEnumerable<char> GetValidDelimiters()
        {
            yield return '\t';
            yield return ' ';
            yield return ',';
            yield return ';';
            yield return '|';
            yield return '~';
            yield return ':';
        }
    }
}