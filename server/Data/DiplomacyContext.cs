using Microsoft.EntityFrameworkCore;

namespace Dipomacy.Data
{
    public partial class DiplomacyContext : DbContext
    {
        public DiplomacyContext(DbContextOptions<DiplomacyContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
