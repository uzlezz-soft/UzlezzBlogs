using Auth.Api.Entities;
using UzlezzBlogs.Core.Dto;

namespace Auth.Api.Mapping;

public static class UserMapping
{
    public static UserProfile ToProfile(this User user) =>
        new UserProfile(user.UserName!, user.Description);
    public static UserProfileDetails ToProfileDetails(this User user) =>
        new UserProfileDetails(user.UserName!, user.DescriptionMarkdown, user.Description);
}
