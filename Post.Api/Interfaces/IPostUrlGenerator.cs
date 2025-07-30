namespace Post.Api.Interfaces;

public interface IPostUrlGenerator
{
    string GenerateUrl(string title, int postId);
}
