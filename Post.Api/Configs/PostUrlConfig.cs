namespace Post.Api.Configs;

public class PostUrlConfig
{
    public const string PostUrl = "PostUrl";

    public int MaxLength { get; set; } = 100;
    public string Delimeter { get; set; } = "-";
    public string Template { get; set; } = "{name}.{id}";
}
