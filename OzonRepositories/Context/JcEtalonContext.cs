using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;

namespace OzonRepositories.Context;

public class JcEtalonContext : DbContext
{
    public DbSet<EtProducer> EtProducers { get; set; }
    public JcEtalonContext(DbContextOptions<JcEtalonContext> options)
        : base(options)
    {
    }
}