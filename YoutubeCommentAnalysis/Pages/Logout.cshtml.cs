using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YoutubeCommentAnalysis.Pages
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            // Oturumu sonlandır
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToPage("/Login");
        }
    }
}
