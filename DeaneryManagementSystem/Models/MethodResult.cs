namespace DeaneryManagementSystem.Models
{
    public record struct MethodResult(bool Status, string? Error = null)
    {
        public static MethodResult Success() => new(true);
        public static MethodResult Failure(string errorMessage) => new(false, errorMessage);
    }
}
