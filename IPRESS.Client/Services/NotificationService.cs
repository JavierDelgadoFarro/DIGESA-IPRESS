using Radzen;

namespace IPRESS.Client.Services
{
    public class NotificationService
    {
        private readonly Radzen.NotificationService? _radzen;

        public NotificationService(Radzen.NotificationService radzen)
        {
            _radzen = radzen;
        }

        public void ShowSuccess(string message)
        {
            _radzen?.Notify(NotificationSeverity.Success, "Éxito", message);
        }

        public void ShowError(string message)
        {
            _radzen?.Notify(NotificationSeverity.Error, "Error", message);
        }

        public void ShowWarning(string message)
        {
            _radzen?.Notify(NotificationSeverity.Warning, "Advertencia", message);
        }

        public void ShowInfo(string message)
        {
            _radzen?.Notify(NotificationSeverity.Info, "Información", message);
        }
    }
}
