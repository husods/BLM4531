using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutubeCommentAnalysis.Services
{
    public interface IYoutubeService
    {
        Task<List<YoutubeComment>> GetCommentsAsync(string videoId, DateTime? startDate = null, DateTime? endDate = null, string keyword = null);
    }

    public class GetYoutubeComments : IYoutubeService
    {
        private readonly YouTubeService _youTubeService;

        public GetYoutubeComments(string apiKey)
        {
            _youTubeService = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = apiKey,
                ApplicationName = "YoutubeCommentAnalysis"
            });
        }

        public async Task<List<YoutubeComment>> GetCommentsAsync(string videoId, DateTime? startDate = null, DateTime? endDate = null, string keyword = null)
        {
            var comments = new List<YoutubeComment>();

            try
            {
                var request = _youTubeService.CommentThreads.List("snippet");
                request.VideoId = videoId;
                request.MaxResults = 100; // Max 100 yorum
                request.Order = CommentThreadsResource.ListRequest.OrderEnum.Relevance; // Yorum sıralama

                var response = await request.ExecuteAsync();

                foreach (var item in response.Items)
                {
                    var commentSnippet = item.Snippet.TopLevelComment.Snippet;

                    // Nullable DateTime kontrolü
                    if (!commentSnippet.PublishedAt.HasValue)
                        continue;

                    var publishedAt = commentSnippet.PublishedAt.Value; // Nullable'dan normal DateTime'a dönüştürme

                    // Tarih filtreleme
                    if (startDate.HasValue && publishedAt < startDate.Value)
                        continue;

                    if (endDate.HasValue && publishedAt > endDate.Value)
                        continue;

                    // Anahtar kelime filtreleme
                    if (!string.IsNullOrWhiteSpace(keyword) &&
                        !commentSnippet.TextDisplay.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        continue;

                    comments.Add(new YoutubeComment
                    {
                        CommentId = item.Id,
                        AuthorName = commentSnippet.AuthorDisplayName,
                        Text = commentSnippet.TextDisplay,
                        Likes = Convert.ToInt32(commentSnippet.LikeCount ?? 0),
                        PublishedAt = publishedAt
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Yorumları alırken bir hata oluştu.", ex);
            }

            return comments;
        }
    }

    public class YoutubeComment
    {
        public string CommentId { get; set; }
        public string AuthorName { get; set; }
        public string Text { get; set; }
        public int Likes { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}
