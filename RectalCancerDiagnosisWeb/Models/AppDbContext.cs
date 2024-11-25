using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RectalCancerDiagnosisWeb.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            //CONSTRUCTER
        }

        public DbSet<User> Users { get; set; }
    }
}
