namespace Blazor.WebApp
{
    public class APIEndpoint
    {
        #region CET/Auth
        public const string CET_Auth_System_Login = "api/v1/cet/auth/systemlogin";
        public const string CET_Auth_RefreshToken = "api/v1/cet/auth/refreshtoken";
        public const string CET_Auth_ConfirmTwoFactor = "api/v1/cet/auth/confirmtwofactor";
        public const string CET_Auth_LogOut = "api/v1/cet/auth/logout";
        public const string CET_Auth_Authentication = "api/v1/cet/auth/authentication";
        public const string CET_Auth_ResetPassword = "api/v1/cet/auth/requestresetpassword";
        #endregion CET/Auth
    }
}