using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web; // HtmlDecode

namespace YoutubeCommentAnalysis.Pages
{
    public class AnalysisModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public AnalysisModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public string VideoUrl { get; set; }

        public bool IsPost { get; set; }
        public bool HasData { get; set; }

        public CharacterDistributionData CharacterDistribution { get; set; }
        public List<WordFrequencyData> WordFrequency { get; set; }
        public TemporalDistributionData TemporalDistribution { get; set; }

        public void OnGet()
        {
            IsPost = false;
            HasData = false;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IsPost = true;

            if (string.IsNullOrWhiteSpace(VideoUrl))
            {
                HasData = false;
                return Page();
            }

            string videoId = ExtractVideoId(VideoUrl);

            if (string.IsNullOrEmpty(videoId))
            {
                HasData = false;
                return Page();
            }

            var comments = await FetchCommentsAsync(videoId, 5000);

            if (comments == null || !comments.Any())
            {
                HasData = false;
                return Page();
            }

            CharacterDistribution = AnalyzeCharacterDistribution(comments);
            WordFrequency = AnalyzeWordFrequency(comments);
            TemporalDistribution = AnalyzeTemporalDistribution(comments);

            HasData = true;
            return Page();
        }

        private string ExtractVideoId(string url)
        {
            if (url.Contains("v="))
            {
                var query = url.Split("v=")[1];
                var ampIndex = query.IndexOf('&');
                return ampIndex != -1 ? query.Substring(0, ampIndex) : query;
            }
            return null;
        }

        private async Task<List<CommentData>> FetchCommentsAsync(string videoId, int maxComments)
        {
            var apiKey = _configuration["YoutubeApi:ApiKey"];
            var apiUrl = $"https://www.googleapis.com/youtube/v3/commentThreads?part=snippet&videoId={videoId}&key={apiKey}&maxResults=100";

            var comments = new List<CommentData>();
            using (var httpClient = new HttpClient())
            {
                string nextPageToken = null;

                do
                {
                    var url = string.IsNullOrEmpty(nextPageToken) ? apiUrl : $"{apiUrl}&pageToken={nextPageToken}";
                    var response = await httpClient.GetStringAsync(url);
                    dynamic jsonData = JsonConvert.DeserializeObject(response);

                    foreach (var item in jsonData.items)
                    {
                        string commentText = item.snippet.topLevelComment.snippet.textDisplay.ToString();
                        string publishedAt = item.snippet.topLevelComment.snippet.publishedAt.ToString();

                        // HTML entity'lerini çöz
                        commentText = HttpUtility.HtmlDecode(commentText);

                        comments.Add(new CommentData
                        {
                            Text = commentText,
                            PublishedAt = DateTime.Parse(publishedAt)
                        });
                    }

                    nextPageToken = jsonData.nextPageToken?.ToString();
                } while (!string.IsNullOrEmpty(nextPageToken) && comments.Count < maxComments);
            }

            return comments;
        }

        private CharacterDistributionData AnalyzeCharacterDistribution(List<CommentData> comments)
        {
            var ranges = new[] { "0-50", "51-100", "101-150", "151-200", "200+" };
            var counts = new int[ranges.Length];

            foreach (var comment in comments)
            {
                var length = comment.Text.Length;
                if (length <= 50) counts[0]++;
                else if (length <= 100) counts[1]++;
                else if (length <= 150) counts[2]++;
                else if (length <= 200) counts[3]++;
                else counts[4]++;
            }

            return new CharacterDistributionData
            {
                Labels = ranges.ToList(),
                Data = counts.ToList()
            };
        }

        private List<WordFrequencyData> AnalyzeWordFrequency(List<CommentData> comments)
        {
            var stopWords = new HashSet<string>
            {
                "the", "and", "a", "to", "is", "in", "it", "you", "of", "for", "on", "this",
                "with", "i", "that", "was", "at", "by", "an", "be", "are", "from", "or", "as",
                "we", "can", "not", "if", "but", "about", "has", "they", "so", "what", "which",
                "their", "all", "there", "how", "when", "who", "will", "my", "your", "do", "me",
                "t", "s", "m", "just", "have", "more", "much", "don", "even", "always", "into",
                "why", "been", "some", "something", "then", "very", "only", "would", "<a", "am",
                "ve", "because", "after", "every"
            };

            var wordCounts = new Dictionary<string, int>();

            var urlRegex = new System.Text.RegularExpressions.Regex(@"https?:\/\/[^\s]+");

            foreach (var comment in comments)
            {
                var cleanedText = urlRegex.Replace(comment.Text, "");

                var words = cleanedText.ToLower()
                                       .Split(' ', '.', ',', '!', '?', ';', ':', '-', '_', '"', '\'', '(', ')')
                                       .Where(w => !string.IsNullOrWhiteSpace(w) && !stopWords.Contains(w));

                foreach (var word in words)
                {
                    if (wordCounts.ContainsKey(word))
                        wordCounts[word]++;
                    else
                        wordCounts[word] = 1;
                }
            }

            return wordCounts.OrderByDescending(w => w.Value)
                             .Take(50)
                             .Select(w => new WordFrequencyData { Word = w.Key, Frequency = w.Value })
                             .ToList();
        }

        private TemporalDistributionData AnalyzeTemporalDistribution(List<CommentData> comments)
        {
            var monthlyCounts = comments.GroupBy(c => new { c.PublishedAt.Year, c.PublishedAt.Month })
                                        .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                                        .Select(g => new
                                        {
                                            Month = $"{g.Key.Year}-{g.Key.Month:00}",
                                            Count = g.Count()
                                        }).ToList();

            return new TemporalDistributionData
            {
                Labels = monthlyCounts.Select(m => m.Month).ToList(),
                Data = monthlyCounts.Select(m => m.Count).ToList()
            };
        }
    }

    public class CommentData
    {
        public string Text { get; set; }
        public DateTime PublishedAt { get; set; }
    }

    public class CharacterDistributionData
    {
        public List<string> Labels { get; set; }
        public List<int> Data { get; set; }
    }

    public class WordFrequencyData
    {
        public string Word { get; set; }
        public int Frequency { get; set; }
    }

    public class TemporalDistributionData
    {
        public List<string> Labels { get; set; }
        public List<int> Data { get; set; }
    }
}
