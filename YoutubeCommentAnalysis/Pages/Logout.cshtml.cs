using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YoutubeCommentAnalysis.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Oturumdan çıkmak: Kullanıcı bilgilerini session'dan kaldırıyoruz
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("UserId");

            // Kullanıcıyı ana sayfaya yönlendiriyoruz
            return RedirectToPage("/Index");
        }
    }
}
