using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YoutubeCommentAnalysis.UsersData;
using System.Linq;

namespace YoutubeCommentAnalysis.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UsersDataContext _context;

        public LoginModel(UsersDataContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // Giriş sayfası yüklendiğinde yapılacak işlemler (şu an boş)
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // E-posta ve şifre boşsa sayfayı yenile
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Lütfen e-posta ve şifreyi girin.";
                return Page();
            }

            // E-posta ve şifreyi kontrol et
            var user = _context.Users.FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user == null)
            {
                // Hata mesajı, kullanıcı bulunamadıysa
                ErrorMessage = "Geçersiz e-posta veya şifre.";
                return Page();
            }

            // Başarılı giriş, kullanıcıyı Session'a kaydetme
            HttpContext.Session.SetString("UserEmail", user.Email); // Kullanıcı e-posta adresini oturuma kaydediyoruz
            HttpContext.Session.SetInt32("UserId", user.Id); // Kullanıcı ID'sini oturuma kaydediyoruz

            // Başarılı giriş, kullanıcıyı yönlendirme
            return RedirectToPage("/Index"); // Ana sayfaya yönlendir
        }
    }
}
