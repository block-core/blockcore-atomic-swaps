using MudBlazor;
using System.ComponentModel;

namespace Blockcore.AtomicSwaps.Client.Services
{
	public interface ISnackbarService
	{
		Task ShowMessage(string message, string position, Severity severity);
	}
}
