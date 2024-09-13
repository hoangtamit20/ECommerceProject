using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace Blazor.WebApp
{
    public class NavigationService
    {
        private readonly NavigationManager _navigationManager;
        private readonly StateContainer _stateContainer;

        public NavigationService(NavigationManager navigationManager, StateContainer stateContainer)
        {
            _navigationManager = navigationManager;
            _stateContainer = stateContainer;
        }

        public async Task TrackNavigationAsync(IJSRuntime jsRuntime)
        {
            // Đăng ký sự kiện khi URL thay đổi
            _stateContainer.PreviousUrl = await JavaScriptHelper.GetCurrentUrlAsync(jsRuntime: jsRuntime);
            _navigationManager.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            _stateContainer.PreviousUrl = e.Location;
        }
    }
}