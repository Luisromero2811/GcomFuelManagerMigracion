using System.Runtime.Serialization;

namespace GComFuelManager.Server.Exceptions
{
    public class UsuarioIdsException : Exception
    {
        public UsuarioIdsException()
        {
        }

        public UsuarioIdsException(string? message) : base(message)
        {
        }

        public UsuarioIdsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UsuarioIdsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
