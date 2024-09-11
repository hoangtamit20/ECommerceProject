using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

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

        public void TrackNavigation()
        {
            // Đăng ký sự kiện khi URL thay đổi
            _navigationManager.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            // Cập nhật PreviousUrl khi URL thay đổi
            _stateContainer.PreviousUrl = e.Location;
        }
    }
}