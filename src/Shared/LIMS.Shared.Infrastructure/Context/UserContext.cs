using System.Text.Json;

namespace LIMS.Shared.Infrastructure.Context;

public class UserContext : LIMS.Shared.Core.Interfaces.IUserContext
{
    public string Username { get; private set; } = "SYSTEM";
    public Guid UserId { get; private set; } = Guid.Empty;

    public void SetUser(string username, Guid userId)
    {
        Username = username;
        UserId = userId;
    }

    public string ChangedData(string notes, Guid? correlationId = null)
    {
        var data = new
        {
            User = Username,
            UserId = UserId,
            Timestamp = DateTime.UtcNow,
            Notes = notes,
            CorrelationId = correlationId ?? Guid.NewGuid()
        };

        return JsonSerializer.Serialize(data);
    }
}
