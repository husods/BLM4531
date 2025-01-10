using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YoutubeCommentAnalysis.UsersData;
using System.Linq;
using System.Threading.Tasks;

namespace YoutubeCommentAnalysis.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UsersDataContext _context;

        public RegisterModel(UsersDataContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Şifreler eşleşiyor mu?
            if (Password != ConfirmPassword)
            {
                ViewData["ErrorMessage"] = "Şifreler eşleşmiyor.";
                return Page();
            }

            // Kullanıcı zaten kayıtlı mı?
            if (_context.Users.Any(u => u.Email == Email))
            {
                ViewData["ErrorMessage"] = "Bu e-posta adresi zaten kullanılıyor.";
                return Page();
            }

            // Yeni kullanıcı oluştur
            var newUser = new User
            {
                Email = Email,
                Password = Password // Şifreleme için daha sonra güncellenecek
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login"); // Kayıt başarılı, giriş sayfasına yönlendir
        }
    }
}
