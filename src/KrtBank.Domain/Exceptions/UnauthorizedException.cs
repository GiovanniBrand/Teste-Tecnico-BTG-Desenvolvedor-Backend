using System.Net;

namespace KrtBank.Domain.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message = "Usuário não autenticado.")
            : base(message, HttpStatusCode.Unauthorized) { }
    }
}
