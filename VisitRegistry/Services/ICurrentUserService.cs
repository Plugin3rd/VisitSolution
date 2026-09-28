namespace VisitRegistry.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string UserDisplayName { get; }
}
