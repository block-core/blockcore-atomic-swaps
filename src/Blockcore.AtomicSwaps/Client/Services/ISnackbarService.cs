using System.ComponentModel;

namespace Blockcore.AtomicSwaps.Client.Services
{
    public interface ISnackbarService
    {

        Task ShowNormalMessage(string message, string position);
        Task ShowInfoMessage(string message, string position);
        Task ShowSuccessMessage(string message, string position);
        Task ShowWarningMessage(string message, string position);
        Task ShowErrorMessage(string message, string position);

    }
}
