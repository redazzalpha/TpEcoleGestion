using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TpGestionEcole.Models
{
    public class EcoleDbEntities:IdentityDbContext
    {

        public EcoleDbEntities(DbContextOptions<EcoleDbEntities> opts): base(opts)
        {

        }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Parcours> Parcours { get; set; }
    }
}
