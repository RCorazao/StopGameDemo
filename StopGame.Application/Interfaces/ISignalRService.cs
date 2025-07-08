namespace StopGame.Application.Interfaces;

public interface ISignalRService
{
    Task SendToGroupAsync(string groupName, string method, object data);
}