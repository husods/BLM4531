using Microsoft.EntityFrameworkCore;

namespace YoutubeCommentAnalysis.UsersData
{
    public class UsersDataContext : DbContext
    {
        public UsersDataContext(DbContextOptions<UsersDataContext> options)
            : base(options)
        {
        }

        // Kullanıcılar tablosu için DbSet tanımı
        public DbSet<User> Users { get; set; }
    }

    // Kullanıcı modeli
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }

}
