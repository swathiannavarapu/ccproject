using Microsoft.EntityFrameworkCore;

namespace YoutubeCategories.Repository
{
    public class ytvideoContext : DbContext
    {
        public ytvideoContext(DbContextOptions<ytvideoContext> options)
            : base(options)
        {
        }

        public DbSet<Regions> Regions { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Videos> Videos { get; set; }
    }
}