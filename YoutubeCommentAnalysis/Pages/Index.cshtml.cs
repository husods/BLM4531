using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  // Logger için
using YoutubeCommentAnalysis.UsersData;

namespace YoutubeCommentAnalysis.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UsersDataContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(UsersDataContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            var user = new User { Email = Email, Password = Password };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login");  // Tekrar ana sayfaya yönlendirme yapar
        }
    }
}
