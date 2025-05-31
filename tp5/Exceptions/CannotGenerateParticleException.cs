using System.Runtime.Serialization;

namespace tp5.Exceptions;

public class CannotGenerateParticleException : Exception
{
    public CannotGenerateParticleException()
    {
    }

    protected CannotGenerateParticleException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public CannotGenerateParticleException(string message) : base(message)
    {
    }

    public CannotGenerateParticleException(string message, Exception innerException) : base(message, innerException)
    {
    }
}