using Microsoft.EntityFrameworkCore;

namespace EquipmentRental.Data;

public class EquipmentRentalDbContext(DbContextOptions<EquipmentRentalDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<EquipmentItem> EquipmentItems => Set<EquipmentItem>();
    public DbSet<RentalRequest> RentalRequests => Set<RentalRequest>();
    public DbSet<RentalRequestItem> RentalRequestItems => Set<RentalRequestItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();

        modelBuilder.Entity<RentalRequestItem>()
            .HasKey(x => new { x.RentalRequestId, x.EquipmentItemId });

        modelBuilder.Entity<RentalRequestItem>()
            .HasOne(x => x.RentalRequest)
            .WithMany(x => x.RentalRequestItems)
            .HasForeignKey(x => x.RentalRequestId);

        modelBuilder.Entity<RentalRequestItem>()
            .HasOne(x => x.EquipmentItem)
            .WithMany(x => x.RentalRequestItems)
            .HasForeignKey(x => x.EquipmentItemId);
    }
}
