using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YoutubeCommentAnalysis.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Message { get; set; }

        public bool IsSubmitted { get; set; }

        public void OnGet()
        {
            IsSubmitted = false;
        }

        public void OnPost()
        {
            // Basit bir gönderim işlemi
            if (!string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(Email) &&
                !string.IsNullOrWhiteSpace(Message))
            {
                IsSubmitted = true;

                // Mesajı kaydetmek veya e-posta göndermek için kod buraya eklenebilir.
                // Örneğin, mesajları bir dosyaya kaydedebilir veya bir e-posta servisi entegre edebilirsiniz.
            }
        }
    }
}
