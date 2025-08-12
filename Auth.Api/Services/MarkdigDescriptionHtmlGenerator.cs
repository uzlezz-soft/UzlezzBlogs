using Auth.Api.Interfaces;
using Markdig;

namespace Auth.Api.Services;

public class MarkdigDescriptionHtmlGenerator : IDescriptionHtmlGenerator
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdigDescriptionHtmlGenerator()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .DisableHeadings()
            .UseAutoLinks()
            .UseBootstrap()
            .UseEmojiAndSmiley()
            .UseEmphasisExtras()
            .UseGlobalization()
            .UseGridTables()
            .UsePipeTables()
            .UseReferralLinks("nofollow")
            .Build();
    }

    public string GenerateHtml(string description) => Markdown.ToHtml(description, _pipeline);
}
