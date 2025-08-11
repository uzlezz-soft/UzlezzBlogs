namespace UzlezzBlogs.Core.Dto;

public record PostPreview(string Title, string Description, string Url, DateTime CreatedDate, string User);
public record RatedPostPreview(string Title, string Description, string Url, DateTime CreatedDate, bool IsUpvote, string User);
public record PostContent(string Title, string Description, string Content, string HtmlContent, string Url);
public record PostDetails(string Id ,string Title, string Description, string Url, string HtmlContent, DateTime CreatedDate, string User, int Views, int CommentCount, bool? Rating, int Upvotes, int Downvotes);

public record PostPreviewList(PostPreview[] Posts, int TotalPages);
public record RatedPostPreviewList(RatedPostPreview[] Posts, int TotalPages);

public record PostCreateRequest(string Title, string Description, string Content);
public record PostEditRequest(string Id, string Description, string Content);

public record PostRatings(int Upvotes, int Downvotes, int Views);

public record PostComment(string User, string Text, DateTime CreatedDate);