using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

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

        private readonly IConfiguration _configuration;

        public ContactModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            IsSubmitted = false;
        }

        public async Task<IActionResult> OnPost()
        {
            if (!string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(Email) &&
                !string.IsNullOrWhiteSpace(Message))
            {
                IsSubmitted = true;

                // SMTP Ayarlarının Yüklenmesi
                var smtpConfig = _configuration.GetSection("SmtpSettings");
                var smtpClient = new SmtpClient(smtpConfig["Host"])
                {
                    Port = int.Parse(smtpConfig["Port"]),
                    Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"]),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpConfig["Username"]),
                    Subject = "Yeni İletişim Mesajı",
                    Body = $"Ad: {Name}\nE-posta: {Email}\nMesaj: {Message}",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(smtpConfig["Username"]);

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    // Hata Yönetimi
                    ModelState.AddModelError(string.Empty, "Mesaj gönderimi sırasında bir hata oluştu.");
                    return Page();
                }
            }

            return Page();
        }
    }
}
