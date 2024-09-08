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

    public enum CTokenProviderType
    {
        None = 0,
        Email = 1,
        Phone = 2,
        Authenticator = 3,
        Default = 4
    }
}