using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Anir.Client.Services.Alerts
{
    public class MudAlertService : IAlertService
    {
        private readonly IDialogService _dialogService;
        private readonly ISnackbar _snackbar;

       
        public MudAlertService(IDialogService dialogService, ISnackbar snackbar)
        {
            _dialogService = dialogService;
            _snackbar = snackbar;
                       
        }

        public async Task<bool> ShowConfirmation(string title, string message)
        {
            bool? result = await _dialogService.ShowMessageBoxAsync(
                title,
                message,
                yesText: "Aceptar",
                cancelText: "Cancelar",
                options: new DialogOptions
                {
                    MaxWidth = MaxWidth.Small,
                    FullWidth = true,
                    BackdropClick = false,
                    CloseButton = false,
                    Position = DialogPosition.Center
                }
            );
            return result == true;
        }

        public Task ShowSuccess(string title, string message)
        {
            // Configuramos la posición y variante DESEADAS
            _snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;
            _snackbar.Configuration.SnackbarVariant = Variant.Filled;
            _snackbar.Configuration.VisibleStateDuration = 3000;

            // Mostramos el mensaje
            _snackbar.Add(message, Severity.Success);

            // Restauramos la configuración original para no afectar a futuras notificaciones
         

            return Task.CompletedTask;
        }

        public Task ShowError(string title, string message)
        {
            _snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;
            _snackbar.Configuration.SnackbarVariant = Variant.Filled;
            _snackbar.Configuration.VisibleStateDuration = 3000;

            _snackbar.Add(message, Severity.Error);

           
            return Task.CompletedTask;
        }

        public Task ShowInformation(string title, string message)
        {
            _snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;
            _snackbar.Configuration.SnackbarVariant = Variant.Filled;
            _snackbar.Configuration.VisibleStateDuration = 3000;

            _snackbar.Add(message, Severity.Info);

           
            return Task.CompletedTask;
        }

        public Task ShowWarning(string title, string message)
        {
            _snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;
            _snackbar.Configuration.SnackbarVariant = Variant.Filled;
            _snackbar.Configuration.VisibleStateDuration = 3000;

            _snackbar.Add(message, Severity.Warning);

            return Task.CompletedTask;
        }
              
    }
}