namespace Post.Api.Interfaces;

public interface IHtmlGenerator
{
    string GenerateHtml(string markdown);
    string GenerateHtmlForComment(string markdown);
}
