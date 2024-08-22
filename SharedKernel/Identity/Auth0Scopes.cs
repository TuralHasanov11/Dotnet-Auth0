namespace SharedKernel.Identity;

public static class Auth0Scopes
{
    public const string Picture = "picture";
    public const string Openid = "openid";
    public const string Profile = "profile";
    public const string Email = "email";
    public const string ViewCourse = "view:course";
    public const string Name = "name";

    public static string[] All => [Picture, Openid, Profile, Email, ViewCourse];
}