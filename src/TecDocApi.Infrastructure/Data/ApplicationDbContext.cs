using Microsoft.EntityFrameworkCore;
using TecDocApi.Domain.Entities;

namespace TecDocApi.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        // Отключаем отслеживание изменений для режима только чтения
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Конфигурация для будущих сущностей будет добавлена здесь
    }

    // Метод SaveChangesAsync удален - доступ только для чтения
}

