using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YoutubeCommentAnalysis.UsersData;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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
            // Giriş sayfası yüklendiğinde yapılacak işlemler
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

            // Kullanıcı bilgilerini Claims olarak ayarla
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("UserId", user.Id.ToString()), // Kullanıcı ID'si
                new Claim(ClaimTypes.Email, user.Email) // E-posta
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Oturumu oluştur
            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
            {
                IsPersistent = true, // Kalıcı oturum
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30) // Oturum süresi
            });

            // Başarılı giriş, kullanıcıyı yönlendirme
            return RedirectToPage("/Index");
        }
    }
}
