namespace SaaSInventoryManagement.Services.Interfaces_
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string Username { get; }
        string CorrelationId { get; }
        string IPAddress { get; }
        string RequestPath { get; }
    }
}