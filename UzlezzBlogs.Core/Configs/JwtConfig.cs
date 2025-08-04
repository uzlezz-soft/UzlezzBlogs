namespace UzlezzBlogs.Core.Configs;

public class JwtConfig
{
    public const string Jwt = "Jwt";

    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int LifetimeDays { get; set; }
}