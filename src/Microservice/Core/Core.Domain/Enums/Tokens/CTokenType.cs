namespace Core.Domain
{
    public enum CTokenType
    {
        None = 0,
        EmailConfirmation = 1,
        PasswordReset = 2,
        TwoFactor = 3,
        ChangeEmail = 4,
        User = 5
    }
}