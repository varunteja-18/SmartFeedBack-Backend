using Microsoft.EntityFrameworkCore;
using FeedbackApp.API.Models;

namespace FeedbackApp.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
    }
}
