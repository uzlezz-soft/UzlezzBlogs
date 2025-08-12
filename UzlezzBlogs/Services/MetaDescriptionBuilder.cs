using System.Text;
using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Services;

public class MetaDescriptionBuilder : IMetaDescriptionBuilder
{
    public string BuildFromPostPreview(IEnumerable<PostPreview> posts, bool includeAuthors = true)
    {
        var description = new StringBuilder();
        foreach (var post in posts)
        {
            description.Append(" | \"");
            if (post.Title.Length > 25)
                description.Append(post.Title.AsSpan().Slice(0, 25)).Append("...");
            else
                description.Append(post.Title);
            if (includeAuthors)
                description.Append("\" by ").Append(post.User);
            else
                description.Append("\" ");
        }
        return description.ToString();
    }
}
