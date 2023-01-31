using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Blockcore.AtomicSwaps.Client.Services.UserPreferences;
using Blockcore.AtomicSwaps.Client.Services;
using MudBlazor;

namespace Blockcore.AtomicSwaps.Client.Shared
{
	public partial class MainLayout : LayoutComponentBase, IDisposable
	{
		[Inject] private LayoutService LayoutService { get; set; }

		private MudThemeProvider _mudThemeProvider;

		protected override void OnInitialized()
		{
			LayoutService.MajorUpdateOccured += LayoutServiceOnMajorUpdateOccured;
			LayoutService.SetBaseTheme(Theme.SwapsTheme());
			base.OnInitialized();
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);

			if (firstRender)
			{
				await ApplyUserPreferences();
				StateHasChanged();
			}
		}

		private async Task ApplyUserPreferences()
		{
			var defaultDarkMode = await _mudThemeProvider.GetSystemPreference();
			await LayoutService.ApplyUserPreferences(defaultDarkMode);
		}

		public void Dispose()
		{
			LayoutService.MajorUpdateOccured -= LayoutServiceOnMajorUpdateOccured;
		}

		private void LayoutServiceOnMajorUpdateOccured(object sender, EventArgs e) => StateHasChanged();

		bool _drawerOpen = true;

		void DrawerToggle()
		{
			_drawerOpen = !_drawerOpen;
		}
	}
}
