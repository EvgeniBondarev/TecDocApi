using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OzonRepositories.Context.Identity
{
    public class OzonIdentityOrderContext : IdentityDbContext<IdentityUser>
    {
        public OzonIdentityOrderContext(DbContextOptions<OzonIdentityOrderContext> options) : base(options) { }

        public virtual DbSet<CustomIdentityUser> CustomIdentityUser { get; set; }
    }
}
