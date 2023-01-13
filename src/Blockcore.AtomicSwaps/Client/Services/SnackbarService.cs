
using MudBlazor;

namespace Blockcore.AtomicSwaps.Client.Services
{
    public class SnackbarService : ISnackbarService
    {
        private readonly ISnackbar _snackbar;

        public SnackbarService(ISnackbar snackbar)
        {
            _snackbar = snackbar;
        }

        /// <summary>
        /// position =  Defaults.Classes.Position.X
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public async Task ShowErrorMessage(string message, string position)
        {
            _snackbar.Clear();
            _snackbar.Configuration.PositionClass = position;
            _snackbar.Add(message, Severity.Error);
        }


        /// <summary>
        /// position =  Defaults.Classes.Position.X
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public async Task ShowInfoMessage(string message, string position)
        {
            _snackbar.Clear();
            _snackbar.Configuration.PositionClass = position;
            _snackbar.Add(message, Severity.Info);
        }


        /// <summary>
        /// position =  Defaults.Classes.Position.X
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public async Task ShowNormalMessage(string message, string position)
        {
            _snackbar.Clear();
            _snackbar.Configuration.PositionClass = position;
            _snackbar.Add(message, Severity.Normal);
        }


        /// <summary>
        /// position =  Defaults.Classes.Position.X
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public async Task ShowSuccessMessage(string message, string position)
        {
            _snackbar.Clear();
            _snackbar.Configuration.PositionClass = position;
            _snackbar.Add(message, Severity.Success);
        }


        /// <summary>
        /// position =  Defaults.Classes.Position.X
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public async Task ShowWarningMessage(string message, string position)
        {
            _snackbar.Clear();
            _snackbar.Configuration.PositionClass = position;
            _snackbar.Add(message, Severity.Warning);
        }
    }
}
