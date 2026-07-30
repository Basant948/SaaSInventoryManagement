using System.Net;

namespace SaaSInventoryManagement.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message)
            : base(message, HttpStatusCode.NotFound) { }

        public NotFoundException(string entityName, object key)
            : base($"{entityName} with id '{key}' was not found.", HttpStatusCode.NotFound) { }
    }
}
