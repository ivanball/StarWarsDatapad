namespace Datapad.Client.Services;

/// <summary>A failed call to the Datapad API. <see cref="Exception.Message"/> is written for the user.</summary>
public sealed class DatapadApiException : Exception
{
    public DatapadApiException()
    {
    }

    public DatapadApiException(string message)
        : base(message)
    {
    }

    public DatapadApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DatapadApiException(string message, bool isNotFound)
        : base(message) => IsNotFound = isNotFound;

    /// <summary>True when retrying cannot help because the requested character does not exist.</summary>
    public bool IsNotFound { get; }
}
