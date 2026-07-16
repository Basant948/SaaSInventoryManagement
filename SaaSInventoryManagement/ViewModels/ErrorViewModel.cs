namespace SaaSInventoryManagement.ViewModels
{
    public class ErrorViewModel
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }

        public bool ShowCorrelationId => !string.IsNullOrEmpty(CorrelationId);
    }
}
