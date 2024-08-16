namespace SpaApp.Identity;

public sealed record UserInfoResponse(
    string Name,
    string Email,
    string ProfileImage,
    string[] Roles,
    string[] Permissions);