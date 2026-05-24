namespace JobPortal.API.Services.Interface;

public interface IChatPresenceService
{
    void RegisterConnection(string connectionId, long userId);

    long? RemoveConnection(string connectionId);

    bool IsUserOnline(long userId);
}
