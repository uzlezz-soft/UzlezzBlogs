using Hydro;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UzlezzBlogs.Pages.Components;

[RequestAuth]
public class RatingComponent(IPostService postService, ILogger<RatingComponent> logger) : HydroComponent
{
    [HtmlAttributeNotBound]
    public string? Token { get; set; }

    public string PostId { get; set; } = default!;

    public bool? Rating { get; set; }

    public int Upvotes { get; set; }
    public int Downvotes { get; set; }

    public override void Mount()
    {
        Token = HttpContext.GetAuthToken()!;
    }

    public Task Upvote() => UpdateRatings(true);

    public Task Downvote() => UpdateRatings(false);

    private async Task UpdateRatings(bool upvote)
    {
        if (Token is null) return;

        if (Rating == null)
        {
            Rating = upvote;
        }
        else
        {
            if (Rating == true && !upvote) Rating = false;
            else if (Rating == false && upvote) Rating = true;
            else Rating = null;
        }

        var response = await postService.RatePost(PostId, upvote ? 1 : 0, Token!);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Unable to update rating to {Rating}: {ErrorStatusCode} {ErrorText}",
                upvote, response.Error.StatusCode, response.Error.Content);
            return;
        }

        var ratings = response.Content!;
        Upvotes = ratings.Upvotes;
        Downvotes = ratings.Downvotes;
    }
}
