using System.ComponentModel;

namespace Core.Domain
{
    public enum CActivityLogType
    {

        None = 0,
        #region Auth
        [Description(description: "User logged into the system using a system account.")]
        SystemLogin = 1,

        [Description(description: "User logged out of the system.")]
        SystemLogout = 2,

        [Description(description: "User failed to log in due to invalid credentials.")]
        FailedLoginAttempt = 3,

        [Description(description: "User changed their password.")]
        PasswordChange = 4,

        [Description(description: "User requested a password reset.")]
        PasswordResetRequest = 5,

        [Description(description: "User successfully reset their password.")]
        PasswordResetSuccess = 6,

        [Description(description: "User's account was locked due to multiple failed login attempts.")]
        AccountLocked = 7,

        [Description(description: "User's account was unlocked.")]
        AccountUnlocked = 8,

        [Description(description: "User's account was deactivated.")]
        AccountDeactivated = 9,

        [Description(description: "User's account was reactivated.")]
        AccountReactivated = 10,

        [Description(description: "User updated their profile information.")]
        ProfileUpdate = 11,

        [Description(description: "User requested a two-factor authentication setup.")]
        TwoFactorAuthSetup = 12,

        [Description(description: "User completed two-factor authentication setup.")]
        TwoFactorAuthCompleted = 13,

        [Description(description: "User disabled two-factor authentication.")]
        TwoFactorAuthDisabled = 14,

        [Description(description: "User was granted new permissions.")]
        PermissionsGranted = 15,

        [Description(description: "User's permissions were revoked.")]
        PermissionsRevoked = 16,

        [Description(description: "User's password was reset by an administrator.")]
        AdminPasswordReset = 17,

        #endregion Auth

    }
}