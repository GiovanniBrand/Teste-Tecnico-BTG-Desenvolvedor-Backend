using System.Net;

namespace KrtBank.Domain.Exceptions
{
    public class RequestTimeoutException : BaseException
    {
        public RequestTimeoutException(string message = "O tempo da requisição expirou.")
            : base(message, HttpStatusCode.RequestTimeout) { }
    }
}
