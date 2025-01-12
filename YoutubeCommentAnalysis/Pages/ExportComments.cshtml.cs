using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using YoutubeCommentAnalysis.Services;

namespace YoutubeCommentAnalysis.Pages
{
    public class ExportModel : PageModel
    {
        private readonly IYoutubeService _youtubeService;
        private readonly string _apiKey;

        [BindProperty]
        public string VideoUrl { get; set; }

        [BindProperty]
        public DateTime? StartDate { get; set; }

        [BindProperty]
        public DateTime? EndDate { get; set; }

        [BindProperty]
        public string Keyword { get; set; }

        [BindProperty]
        public bool IncludeName { get; set; }

        [BindProperty]
        public bool IncludeComment { get; set; }

        [BindProperty]
        public bool IncludeLikes { get; set; }

        [BindProperty]
        public bool IncludeDate { get; set; }

        [BindProperty]
        public bool IncludeCommentId { get; set; }

        [BindProperty]
        public string UseKeyword { get; set; }

        public string CsvFilePath { get; private set; }

        // Constructor to inject YouTube service and API key
        public ExportModel(IConfiguration configuration, IYoutubeService youtubeService)
        {
            _apiKey = configuration.GetValue<string>("YoutubeApi:ApiKey");
            _youtubeService = youtubeService;
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            if (string.IsNullOrWhiteSpace(VideoUrl))
            {
                ModelState.AddModelError(nameof(VideoUrl), "YouTube Video Linki gerekli.");
                return Page();
            }

            if (UseKeyword == "Yes" && string.IsNullOrWhiteSpace(Keyword))
            {
                ModelState.AddModelError(nameof(Keyword), "Anahtar kelime girilmesi gerekiyor.");
                return Page();
            }

            try
            {
                // Video ID dışa aktarma
                var videoId = ExtractVideoId(VideoUrl);
                var comments = await _youtubeService.GetCommentsAsync(videoId, StartDate, EndDate, Keyword);

                // Dosya yolu oluşturma
                var filePath = Path.Combine(Path.GetTempPath(), "comments.csv");

                // Seçili sütünları CSV dosyasına yazdırma
                await WriteCommentsToCsvAsync(comments, filePath);
                CsvFilePath = filePath;

                // İndirmeyi başlat
                return File(System.IO.File.ReadAllBytes(CsvFilePath), "application/csv", "comments.csv");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Bir hata oluştu: {ex.Message}");
                return Page();
            }
        }

        private string ExtractVideoId(string videoUrl)
        {
            // YouTube URL'den video ID'yi dışa aktarma
            var regex = new System.Text.RegularExpressions.Regex(@"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|\S*\?v=)|(?:youtu\.be\/))([a-zA-Z0-9_-]+)");
            var match = regex.Match(videoUrl);
            return match.Success ? match.Groups[1].Value : throw new Exception("Geçerli bir YouTube URL'si girin.");
        }

        private string EscapeCsvField(string field)
        {
            if (field.Contains("\""))
            {
                field = field.Replace("\"", "\"\"");
            }
            if (field.Contains(",") || field.Contains("\n"))
            {
                field = $"\"{field}\"";
            }
            return field;
        }


        private async Task WriteCommentsToCsvAsync(List<YoutubeComment> comments, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Geçerli bir dosya yolu sağlanamadı.");
            }

            using (var writer = new StreamWriter(filePath))
            {
                // Seçilen sütunlara göre CSV başlığını yazdır
                List<string> headers = new List<string>();

                if (IncludeCommentId) headers.Add("CommentId");
                if (IncludeName) headers.Add("AuthorName");
                if (IncludeComment) headers.Add("Comment");
                if (IncludeLikes) headers.Add("Likes");
                if (IncludeDate) headers.Add("PublishedAt");

                // Başlık satırını yazdır
                writer.WriteLine(string.Join(",", headers));

                // Seçilen sütunlara göre satırları yazdırma
                foreach (var comment in comments)
                {
                    List<string> row = new List<string>();

                    // HTML Etiketlerini Temizle
                    string cleanedComment = RemoveHtmlTags(HttpUtility.HtmlDecode(comment.Text));

                    if (IncludeCommentId) row.Add(EscapeCsvField(HttpUtility.HtmlDecode(comment.CommentId.ToString())));
                    if (IncludeName) row.Add(EscapeCsvField(HttpUtility.HtmlDecode(comment.AuthorName)));
                    if (IncludeComment) row.Add(EscapeCsvField(cleanedComment));
                    if (IncludeLikes) row.Add(EscapeCsvField(HttpUtility.HtmlDecode(comment.Likes.ToString())));
                    if (IncludeDate) row.Add(EscapeCsvField(HttpUtility.HtmlDecode(comment.PublishedAt.ToString("yyyy-MM-dd"))));

                    // Satırı yazdır
                    writer.WriteLine(string.Join(",", row));
                }
            }
        }

        private string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // HTML etiketlerini temizlemek için regex
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        }


        public IActionResult OnGetDownloadCsv(bool download)
        {
            if (download && !string.IsNullOrEmpty(CsvFilePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(CsvFilePath);
                var fileName = "comments.csv";
                return File(fileBytes, "application/csv", fileName);
            }
            return NotFound();
        }
    }
}
