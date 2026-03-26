namespace Anir.Client.Services.Alerts
{
    public interface IAlertService
    {
        Task<bool> ShowConfirmation(string title, string message);
        Task ShowSuccess(string title, string message);
        Task ShowError(string title, string message);
        Task ShowInformation(string title, string message);
        Task ShowWarning(string title, string message);
    }
}
