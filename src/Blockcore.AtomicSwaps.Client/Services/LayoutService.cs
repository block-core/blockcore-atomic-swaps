using Blockcore.AtomicSwaps.Client.Services.UserPreferences;
using MudBlazor;

namespace Blockcore.AtomicSwaps.Client.Services;

public class LayoutService
{
	private readonly IUserPreferencesService _userPreferencesService;
	private UserPreferences.UserPreferences _userPreferences;

	public bool IsDarkMode { get; private set; } = true;

	public MudTheme CurrentTheme { get; private set; }


	public LayoutService(IUserPreferencesService userPreferencesService)
	{
		_userPreferencesService = userPreferencesService;
	}

	public void SetDarkMode(bool value)
	{
		IsDarkMode = value;
	}

	public async Task ApplyUserPreferences(bool isDarkModeDefaultTheme)
	{
		_userPreferences = await _userPreferencesService.LoadUserPreferences();
		if (_userPreferences != null)
		{
			IsDarkMode = _userPreferences.DarkTheme;
		}
		else
		{
			IsDarkMode = isDarkModeDefaultTheme;
			_userPreferences = new UserPreferences.UserPreferences { DarkTheme = IsDarkMode };
			await _userPreferencesService.SaveUserPreferences(_userPreferences);
		}
	}

	public event EventHandler MajorUpdateOccured;

	private void OnMajorUpdateOccured() => MajorUpdateOccured?.Invoke(this, EventArgs.Empty);

	public async Task ToggleDarkMode()
	{
		IsDarkMode = !IsDarkMode;
		_userPreferences.DarkTheme = IsDarkMode;
		await _userPreferencesService.SaveUserPreferences(_userPreferences);
		OnMajorUpdateOccured();
	}

	public void SetBaseTheme(MudTheme theme)
	{
		CurrentTheme = theme;
		OnMajorUpdateOccured();
	}

}
