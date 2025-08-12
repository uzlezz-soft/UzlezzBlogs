using Markdig;
using Markdig.SyntaxHighlighting;
using Post.Api.Interfaces;
using System.Text.RegularExpressions;

namespace Post.Api.Services;

public class MarkdigHtmlGenerator : IHtmlGenerator
{
    private MarkdownPipeline _pipeline;
    private MarkdownPipeline _commentPipeline;

    public MarkdigHtmlGenerator()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseBootstrap()
            .UseSyntaxHighlighting()
            .UseReferralLinks("nofollow")
            .UseGlobalization()
            .UsePreciseSourceLocation()
            .DisableHtml()
            .Build();

        _commentPipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .DisableHeadings()
            .UseGlobalization()
            .UseReferralLinks("nofollow")
            .Build();
    }

    public string GenerateHtml(string markdown) => Markdown.ToHtml(markdown ?? string.Empty, _pipeline);

    public string GenerateHtmlForComment(string markdown)
    {
        var html = Markdown.ToHtml(markdown ?? string.Empty, _commentPipeline);
        return Regex.Replace(html, "<img[^>]*>", "", RegexOptions.IgnoreCase);
    }
}

