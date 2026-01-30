using System.Net;

namespace KrtBank.Domain.Exceptions
{
    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string message = "Acesso proibido para este perfil.")
            : base(message, HttpStatusCode.Forbidden) { }
    }
}
