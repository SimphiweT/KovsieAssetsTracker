using KovsieAssetsTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Campus> Campuses { get; set; }
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Floor> Floors { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetCategory> AssetCategories { get; set; }
    public DbSet<UserAccessRequest> UserAccessRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table names (plural to match DbSet)
        modelBuilder.Entity<Campus>().ToTable("Campuses");
        modelBuilder.Entity<Building>().ToTable("Buildings");
        modelBuilder.Entity<Floor>().ToTable("Floors");
        modelBuilder.Entity<Room>().ToTable("Rooms");
        modelBuilder.Entity<Asset>().ToTable("Assets");
        modelBuilder.Entity<AssetCategory>().ToTable("AssetCategories");
        modelBuilder.Entity<UserAccessRequest>().ToTable("UserAccessRequests");

        // User constraints
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.UsernameDisplay).IsUnique();

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.StudentStaffNumber).IsUnique();

        /*---------------- Relationships ----------------*/

        modelBuilder.Entity<Campus>()
            .HasMany(c => c.Buildings)
            .WithOne(b => b.Campus)
            .HasForeignKey(b => b.CampusServerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Building>()
            .HasMany(b => b.Floors)
            .WithOne(f => f.Building)
            .HasForeignKey(f => f.BuildingServerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Floor>()
            .HasMany(f => f.Rooms)
            .WithOne(r => r.Floor)
            .HasForeignKey(r => r.FloorServerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Room>()
            .HasMany(r => r.Assets)
            .WithOne(d => d.Room)
            .HasForeignKey(d => d.RoomServerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Asset>()
            .HasOne(a => a.AssetCategory)
            .WithMany(ac => ac.Assets)
            .HasForeignKey(a => a.AssetCategoryServerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
