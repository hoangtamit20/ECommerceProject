using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;

namespace Blazor.WebApp
{
    public class AuthenticationHelper
    {
        public static async Task AuthenticationAsync(CLoadingStatus loadingStatus, ApiClient apiClient,
            StateContainer stateContainer, NavigationManager navigationManager,
            LocalStorageService localStorage, IToastService toastService)
        {
            loadingStatus.IsLoading = true;
            try
            {
                if (await apiClient.IsAuthentication())
                {
                    loadingStatus.IsLoading = false;
                    stateContainer.Message = "You are already login";
                    navigationManager.NavigateTo(uri: stateContainer.PreviousUrl ?? "/");
                }
                else
                {
                    var isRefreshToken = await localStorage.IsRefreshTokenAsync(apiClient);
                    loadingStatus.IsLoading = false;
                    if (isRefreshToken)
                    {
                        stateContainer.Message = "You are already login";
                        navigationManager.NavigateTo(uri: stateContainer.PreviousUrl ?? "/");
                    }
                }
            }
            catch (Exception ex)
            {
                loadingStatus.IsLoading = false;
                toastService.ShowError(ex.Message);
            }
        }

        public static async Task<bool> IsAuthenticationAsync(ApiClient apiClient, LocalStorageService localStorage)
        {
            var isAuthen = await apiClient.IsAuthentication();
            if (isAuthen)
            {
                return true;
            }
            var isRefresh = await localStorage.IsRefreshTokenAsync(apiClient: apiClient);
            if (isRefresh)
            {
                return true;
            }
            return false;
        }
    }
}