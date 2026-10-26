namespace Datapad.Api.Swapi;

/// <summary>SWAPI could not be reached, timed out, returned an error status, or returned a body we cannot read.</summary>
public sealed class SwapiUnavailableException : Exception
{
    public SwapiUnavailableException()
    {
    }

    public SwapiUnavailableException(string message)
        : base(message)
    {
    }

    public SwapiUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
