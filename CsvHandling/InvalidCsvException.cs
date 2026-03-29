using System;

namespace DoenaSoft.CsvHandling;

/// <summary>
/// Exception thrown when CSV file parsing fails due to invalid format
/// </summary>
public sealed class InvalidCsvException : Exception
{
    /// <summary>
    /// Creates a new InvalidCsvException with the specified error message
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    public InvalidCsvException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new InvalidCsvException with the specified error message and inner exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public InvalidCsvException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a new InvalidCsvException with diagnostic information
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="lineNumber">The 1-based line number where the error occurred (0 if unknown)</param>
    /// <param name="charPosition">The character position within the file where the error occurred (0 if unknown)</param>
    public InvalidCsvException(string message, int lineNumber, int charPosition) : base(FormatMessage(message, lineNumber, charPosition))
    {
        LineNumber = lineNumber;
        CharPosition = charPosition;
    }

    /// <summary>
    /// Creates a new InvalidCsvException with diagnostic information and inner exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="lineNumber">The 1-based line number where the error occurred (0 if unknown)</param>
    /// <param name="charPosition">The character position within the file where the error occurred (0 if unknown)</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public InvalidCsvException(string message, int lineNumber, int charPosition, Exception innerException) 
        : base(FormatMessage(message, lineNumber, charPosition), innerException)
    {
        LineNumber = lineNumber;
        CharPosition = charPosition;
    }

    /// <summary>
    /// Gets the 1-based line number where the error occurred, or 0 if unknown
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the character position within the file where the error occurred, or 0 if unknown
    /// </summary>
    public int CharPosition { get; }

    private static string FormatMessage(string message, int lineNumber, int charPosition)
    {
        if (lineNumber > 0 && charPosition > 0)
        {
            return $"{message} (Line: {lineNumber}, Position: {charPosition})";
        }
        else if (lineNumber > 0)
        {
            return $"{message} (Line: {lineNumber})";
        }
        else if (charPosition > 0)
        {
            return $"{message} (Position: {charPosition})";
        }
        else
        {
            return message;
        }
    }
}