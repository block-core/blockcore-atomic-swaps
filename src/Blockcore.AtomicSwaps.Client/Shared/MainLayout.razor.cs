using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Blockcore.AtomicSwaps.Client.Services.UserPreferences;
using Blockcore.AtomicSwaps.Client.Services;
using MudBlazor;
using Blockcore.AtomicSwaps.Client.Pages.Dialogs;
using Microsoft.JSInterop;
using NLog.Config;

namespace Blockcore.AtomicSwaps.Client.Shared
{
    public partial class MainLayout : LayoutComponentBase, IDisposable
    {
        [Inject] private LayoutService LayoutService { get; set; }

        private MudThemeProvider _mudThemeProvider;
        static Action OnInstallable;
        protected override void OnInitialized()
        {
            LayoutService.MajorUpdateOccured += LayoutServiceOnMajorUpdateOccured;
            LayoutService.SetBaseTheme(Theme.Theme.SwapsTheme());

            OnInstallable = async () =>
            {
                var parameters = new DialogParameters();
                var options = new DialogOptions() { CloseButton = false, NoHeader = true, MaxWidth = MaxWidth.Small, Position = DialogPosition.BottomCenter };
                var dialog = _dialogService.Show<InstallApp>("", parameters, options);
                var result = await dialog.Result;
                if (!result!.Canceled)
                {
                    await _jsRuntime.InvokeVoidAsync("BlazorPWA.installPWA");
                }
            };

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
        private void OpenDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true };
            DialogService.Show<Github>("Github", options);
        }

        [JSInvokable("PWAInstallable")]
        public static Task PWAInstallable()
        {
            OnInstallable.Invoke();
            return Task.CompletedTask;
        }

        [JSInvokable("ShowUpdateVersion")]
        public Task ShowUpdateVersion()
        {
            _snackBar.Configuration.PositionClass = Defaults.Classes.Position.BottomCenter;
            var message = "New version available.";
            _snackBar.Add(message, Severity.Info, config =>
            {
                config.RequireInteraction = true;
                config.ShowCloseIcon = false;
                config.Action = "UPDATE?";
                config.Onclick = async (snackbar) =>
                {
                    await _jsRuntime.InvokeVoidAsync("AtomicSwaps.onUserUpdate");
                };
            });
            return Task.CompletedTask;
        }


    }
}
