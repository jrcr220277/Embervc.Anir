using Microsoft.JSInterop;

namespace Anir.Client.Services.Alerts
{
    public class SweetAlertService : IAlertService
    {
        private readonly IJSRuntime _js;

        public SweetAlertService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<bool> ShowConfirmation(string title, string message)
        {
            var result = await _js.InvokeAsync<AlertResult>("Swal.fire", new
            {
                title,
                text = message,
                icon = "warning",
                showCancelButton = true,
                confirmButtonText = "Sí",
                cancelButtonText = "No",
                reverseButtons = true,
                buttonsStyling = false,
                customClass = new
                {
                    actions = "d-flex justify-content-center gap-3",
                    confirmButton = "btn btn-success w-45",
                    cancelButton = "btn btn-outline-secondary w-45 sweet-alert-no-focus"
                }
            });

            return result.IsConfirmed;
        }

        public async Task ShowSuccess(string title, string message)
        {
            await _js.InvokeVoidAsync("Swal.fire", new
            {
                icon = "success",
                title,
                text = message
            });
        }

        public async Task ShowError(string title, string message)
        {
            await _js.InvokeVoidAsync("Swal.fire", new
            {
                icon = "error",
                title,
                text = message
            });
        }

        public async Task ShowInformation(string title, string message)
        {
            await _js.InvokeVoidAsync("Swal.fire", new
            {
                icon = "info",
                title,
                text = message
            });
        }

        public async Task ShowWarning(string title, string message)
        {
            await _js.InvokeVoidAsync("Swal.fire", new
            {
                icon = "warning",
                title,
                text = message
            });
        }

    }

    public class AlertResult
    {
        public bool IsConfirmed { get; set; }
        public bool IsDenied { get; set; }
        public bool IsDismissed { get; set; }
    }


}
