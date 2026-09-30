using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VisitRegistry.Models;

namespace VisitRegistry.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<OrganizationalUnit> OrganizationalUnits => Set<OrganizationalUnit>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<BlackListEntry> BlackLists => Set<BlackListEntry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateRecordVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        UpdateRecordVersions();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateRecordVersions()
    {
        foreach (var entry in ChangeTracker.Entries<AuditableBase>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            entry.Entity.RecordVersion = Guid.NewGuid().ToByteArray();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasIndex(x => x.NationalCode).IsUnique();
            entity.Property(x => x.RecordVersion).IsConcurrencyToken();
            entity.HasOne(x => x.OrganizationalUnit)
                .WithMany(x => x.Persons)
                .HasForeignKey(x => x.OrganizationalUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrganizationalUnit>(entity =>
        {
            entity.HasIndex(x => x.Title).IsUnique();
            entity.Property(x => x.RecordVersion).IsConcurrencyToken();
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.Property(x => x.RecordVersion).IsConcurrencyToken();
            entity.HasOne(x => x.Person)
                .WithMany(x => x.Visits)
                .HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VisitingUnit)
                .WithMany(x => x.Visits)
                .HasForeignKey(x => x.VisitingUnitId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.PersonId, x.ExitDate, x.ExitTime });
        });

        modelBuilder.Entity<BlackListEntry>(entity =>
        {
            entity.Property(x => x.RecordVersion).IsConcurrencyToken();
            entity.HasOne(x => x.Person)
                .WithMany(x => x.BlackListEntries)
                .HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
