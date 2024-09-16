using Microsoft.AspNetCore.Components;

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

        public async Task TrackNavigationAsync()
        {
            var uri = _navigationManager.Uri;
            _stateContainer.PreviousUrl = uri;
            await Task.CompletedTask;
        }
    }
}