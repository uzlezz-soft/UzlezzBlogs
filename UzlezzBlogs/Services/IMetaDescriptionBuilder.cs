using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Services;

public interface IMetaDescriptionBuilder
{
    public string BuildFromPostPreview(IEnumerable<PostPreview> posts, bool includeAuthors = true);
}
