using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YoutubeCommentAnalysis.UsersData;

namespace YoutubeCommentAnalysis.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UsersDataContext _context;

        public IndexModel(UsersDataContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public void OnGet() { }
    }
}
