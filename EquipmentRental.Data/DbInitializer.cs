namespace EquipmentRental.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(EquipmentRentalDbContext db)
    {
        if (db.Roles.Any())
        {
            return;
        }

        var adminRole = new Role { Name = "Admin" };
        var userRole = new Role { Name = "User" };

        db.Roles.AddRange(adminRole, userRole);
        await db.SaveChangesAsync();

        db.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = PasswordHasher.Hash("admin123"),
            FirstName = "System",
            LastName = "Admin",
            RoleId = adminRole.Id
        });

        db.EquipmentItems.AddRange(
            new EquipmentItem
            {
                Name = "Projector",
                Description = "Full HD projector",
                AvailableQuantity = 5,
                ImageUrl = "https://i8.amplience.net/i/epsonemear/qlseries_b_std_03_png?$product-xlarge$&fmt=auto&img404=missing_product&v=1",
                Condition = EquipmentCondition.Used
            },
            new EquipmentItem
            {
                Name = "Laptop",
                Description = "15-inch business laptop",
                AvailableQuantity = 4,
                ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=1200&q=60",
                Condition = EquipmentCondition.Used
            },
            new EquipmentItem
            {
                Name = "Camera",
                Description = "DSLR camera",
                AvailableQuantity = 3,
                ImageUrl = "https://cdn.mos.cms.futurecdn.net/QmY5fRUXJEtnzhTgXLSY5A-1200-80.jpg",
                Condition = EquipmentCondition.ForRepair
            });

        await db.SaveChangesAsync();
    }
}
