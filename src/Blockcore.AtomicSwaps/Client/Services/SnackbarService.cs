
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
		public async Task ShowMessage(string message, string position, Severity severity)
		{
			_snackbar.Clear();
			_snackbar.Configuration.PositionClass = position;
			_snackbar.Add(message, severity);
		}

	}
}
