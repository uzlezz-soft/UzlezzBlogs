using Microsoft.Extensions.Options;
using Post.Api.Configs;
using Post.Api.Interfaces;
using System.Text;

namespace Post.Api.Services;

public class PostUrlGenerator : IPostUrlGenerator
{
    private readonly PostUrlConfig _config;

    public PostUrlGenerator(IOptions<PostUrlConfig> config)
    {
        _config = config.Value;
    }

    public string GenerateUrl(string title, int postId)
    {
        StringBuilder stringBuilder = new();
        foreach (var c in title
            .ToLower()
            .Where(x => char.IsLetterOrDigit(x) || char.IsPunctuation(x) || x == ' ')
            .Take(_config.MaxLength)
            .Select(x => (x == ' ' || char.IsPunctuation(x)) ? _config.Delimeter[0] : char.ToLowerInvariant(x)))
        {
            stringBuilder.Append(c);
        }
        return string.Format(_config.Template, stringBuilder.ToString(), postId);
    }
}
