namespace Anir.Shared.Contracts.User;

public class UserBulkActionRequest
{
    public List<string> Ids { get; set; } = new();
}
