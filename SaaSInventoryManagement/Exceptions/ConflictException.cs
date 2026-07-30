using System.Net;

namespace SaaSInventoryManagement.Exceptions
{
    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, HttpStatusCode.Conflict) { }
    }
}
