using System.Net;

namespace KrtBank.Domain.Exceptions
{
    public class BusinessException : BaseException
    {
        public BusinessException(string message) : base(message, HttpStatusCode.BadRequest) { }
    }
}
