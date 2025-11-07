using KovsieAssetsTracker.Application.Helpers;
using KovsieAssetsTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KovsieAssetsTracker.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider services, int maxRetries = 3)
        {
            int attempts = 0;
            while (attempts < maxRetries)
            {
                try
                {
                    using var scope = services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    // Apply migrations
                    if ((await context.Database.GetPendingMigrationsAsync()).Any())
                    {
                        Console.WriteLine("Applying pending migrations...");
                        await context.Database.MigrateAsync();
                    }

                    // Seed everything
                    await SeedRolesAsync(roleMgr);
                    await SeedUsersAsync(userMgr);
                    await SeedCampusesAsync(context);
                    await SeedBuildingsAsync(context);
                    await SeedFloorsAsync(context);
                    await SeedRoomsAsync(context);
                    await SeedAssetCategoriesAsync(context);
                    await SeedAssetsAsync(context);

                    Console.WriteLine("Database seeding completed successfully.");
                    return;
                }
                catch (Exception ex)
                {
                    attempts++;
                    Console.WriteLine($"Seeding attempt {attempts} failed: {ex.Message}");
                    if (attempts >= maxRetries)
                        throw;
                    await Task.Delay(2000); // wait 2 seconds before retry
                }
            }
        }

        #region Roles & Users

        private static readonly SeedUser[] seedUsers = new[]
        {
            new SeedUser
            {
                UsernameDisplay = "Kovsie@Admin",
                Email = "2016401495@ufs4life.ac.za",
                StudentStaffNumber = "2016401495",
                ProfileImage = "admin.png",
                ContactNumber = "0849702454",
                Role = ConstantsUtils.ADMIN_ROLE,
                Password = "Admin@1234",
                Description = "Administrator of KovsieAssetManager, responsible for managing users."
            },
            new SeedUser
            {
                UsernameDisplay = "Staff@Admin",
                Email = "simphiweterrence@yahoo.com",
                StudentStaffNumber = "2016401494",
                ProfileImage = "staff.png",
                ContactNumber = "0765701041",
                Role = ConstantsUtils.STAFF_ROLE,
                Password = "Staff@1234",
                Description = "Staff member of KovsieAssetManager, responsible for managing assets."
            },
            new SeedUser
            {
                UsernameDisplay = "Agent@Admin",
                Email = "simphiweterrencenxumalo@gmail.com",
                StudentStaffNumber = "2016401493",
                ProfileImage = "default-user.png",
                ContactNumber = "0849702454",
                Role = ConstantsUtils.AGENT_ROLE,
                Password = " Agent@1234",
                Description = "Verification Agent of KovsieAssetManager, responsible for verifying assets."
            }
        };

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleMgr)
        {
            string[] roles = seedUsers.Select(u => u.Role).Distinct().ToArray();

            foreach (var role in roles)
            {
                if (!await roleMgr.RoleExistsAsync(role))
                {
                    var result = await roleMgr.CreateAsync(new IdentityRole(role));
                    Console.WriteLine(result.Succeeded
                        ? $"UserRole '{role}' created successfully."
                        : $"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                else
                {
                    Console.WriteLine($"UserRole '{role}' already exists. Skipping.");
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userMgr)
        {
            foreach (var userInfo in seedUsers)
            {
                var existingUser = await userMgr.FindByEmailAsync(userInfo.Email);

                if (existingUser != null)
                {
                    Console.WriteLine($"User {userInfo.UsernameDisplay} already exists. Ensuring role '{userInfo.Role}' is assigned...");
                    if (!await userMgr.IsInRoleAsync(existingUser, userInfo.Role))
                    {
                        var roleResult = await userMgr.AddToRoleAsync(existingUser, userInfo.Role);
                        Console.WriteLine(roleResult.Succeeded
                            ? $"UserRole '{userInfo.Role}' assigned to existing user {userInfo.UsernameDisplay}."
                            : $"Failed to assign role '{userInfo.Role}' to user {userInfo.UsernameDisplay}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                    continue;
                }

                var user = new ApplicationUser
                {
                    UserName = userInfo.Email,
                    UsernameDisplay = userInfo.UsernameDisplay,
                    Email = userInfo.Email,
                    StudentStaffNumber = userInfo.StudentStaffNumber,
                    ProfileImageFileName = userInfo.ProfileImage,
                    ContactNumber = userInfo.ContactNumber,
                    Description = userInfo.Description,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await userMgr.CreateAsync(user, userInfo.Password);

                if (result.Succeeded)
                {
                    var roleResult = await userMgr.AddToRoleAsync(user, userInfo.Role);
                    Console.WriteLine(roleResult.Succeeded
                        ? $"{userInfo.UsernameDisplay} created successfully with role '{userInfo.Role}'."
                        : $"User created but failed to assign role '{userInfo.Role}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
                else
                {
                    Console.WriteLine($"Failed to create user {userInfo.UsernameDisplay}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private class SeedUser
        {
            public string UsernameDisplay { get; set; }
            public string Email { get; set; }
            public string StudentStaffNumber { get; set; }
            public string ProfileImage { get; set; }
            public string ContactNumber { get; set; }
            public string Role { get; set; }
            public string Password { get; set; }
            public string Description { get; set; }
        }

        #endregion

        #region Entity Seeding

        private static async Task SeedCampusesAsync(AppDbContext context)
        {
            if (await context.Campuses.AnyAsync()) return;

            var campuses = new[]
            {
                new Campus { CampusName = "QwaQwa Campus", CampusDescription = "Based in Free State, QwaQwa Around beautiful Mountains", CampusImage = "qwaqwa_campus.jpg", IsAndroidBackedUp = false, IsServerBackedUp = true, CreatedDate = DateTime.UtcNow },
                new Campus { CampusName = "Bloemfontein Campus", CampusDescription = "Based in Free State, Bloemfontein Around beautiful City", CampusImage = "blomfontein_campus.jpg", IsAndroidBackedUp = false, IsServerBackedUp = true, CreatedDate = DateTime.UtcNow },
                new Campus { CampusName = "South Campus", CampusDescription = "Based in Free State, Welkom Around beautiful towns", CampusImage = "south_campus.jpg", IsAndroidBackedUp = false, IsServerBackedUp = true, CreatedDate = DateTime.UtcNow }
            };

            await context.Campuses.AddRangeAsync(campuses);
            await context.SaveChangesAsync();
            Console.WriteLine("Campuses seeded.");
        }

        private static async Task SeedBuildingsAsync(AppDbContext context)
        {
            if (await context.Buildings.AnyAsync()) return;

            var buildings = new[]
            {
                new Building { BuildingName = "Mandel Hall", BuildingDescription = "Good storage for printers and chairs", BuildingImage = "mandela_hall.jpg", IsAndroidBackedUp = false, IsServerBackedUp = true, CreatedDate = DateTime.UtcNow },
                new Building { BuildingName = "Science Building", BuildingDescription = "Good storage for laptops and laboratory tools", BuildingImage = "ufs_science_building.jpg", IsAndroidBackedUp = false, IsServerBackedUp = true, CreatedDate = DateTime.UtcNow }
            };

            await context.Buildings.AddRangeAsync(buildings);
            await context.SaveChangesAsync();
            Console.WriteLine("Buildings seeded.");
        }

        private static async Task SeedFloorsAsync(AppDbContext context)
        {
            if (await context.Floors.AnyAsync()) return;

            var floors = new[]
            {
                new Floor { FloorName = "Ground Floor", FloorDescription = "Clean floor with many rooms", IsAndroidBackedUp = false, IsServerBackedUp = true, CreatedDate = DateTime.UtcNow },
                new Floor { FloorName = "First Floor", FloorDescription = "Clean floor with many rooms", IsAndroidBackedUp = false, IsServerBackedUp = true, CreatedDate = DateTime.UtcNow }
            };

            await context.Floors.AddRangeAsync(floors);
            await context.SaveChangesAsync();
            Console.WriteLine("Floors seeded.");
        }

        private static async Task SeedRoomsAsync(AppDbContext context)
        {
            if (await context.Rooms.AnyAsync()) return;

            var rooms = new[]
            {
                new Room { RoomName = "Computer Lab", RoomDescription = "Suited for network and computers", RoomImage = "default-room.jpg", IsAndroidBackedUp = false, IsServerBackedUp = true, CreatedDate = DateTime.UtcNow },
                new Room { RoomName = "B0024", RoomDescription = "Storage for printers", RoomImage = "default-room.jpg", IsAndroidBackedUp = false, IsServerBackedUp = true, CreatedDate = DateTime.UtcNow }
            };

            await context.Rooms.AddRangeAsync(rooms);
            await context.SaveChangesAsync();
            Console.WriteLine("Rooms seeded.");
        }

        private static async Task SeedAssetCategoriesAsync(AppDbContext context)
        {
            if (await context.AssetCategories.AnyAsync()) return;

            var categories = new[]
            {
                new AssetCategory { AssetCategoryName = "Laptops", AssetCategoryDescription = "Collection for laptops", AssetCategoryImage = "default-assetcategory.png", CreatedDate = DateTime.UtcNow },
                new AssetCategory { AssetCategoryName = "Desktops", AssetCategoryDescription = "Collection for desktops", AssetCategoryImage = "default-assetcategory.png", CreatedDate = DateTime.UtcNow },
                new AssetCategory { AssetCategoryName = "Mobiles", AssetCategoryDescription = "Collection for mobile phones", AssetCategoryImage = "default-assetcategory.png", CreatedDate = DateTime.UtcNow },
                new AssetCategory { AssetCategoryName = "Furniture", AssetCategoryDescription = "Collection for Furniture", AssetCategoryImage = "default-assetcategory.png", CreatedDate = DateTime.UtcNow },
                new AssetCategory { AssetCategoryName = "Network", AssetCategoryDescription = "Collection for networking tools", AssetCategoryImage = "default-assetcategory.png", CreatedDate = DateTime.UtcNow }
            };

            await context.AssetCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            Console.WriteLine("Asset categories seeded.");
        }

        private static async Task SeedAssetsAsync(AppDbContext context)
        {
            if (await context.Assets.AnyAsync()) return;

            var assets = new[]
            {
                new Asset { AssetName = "Asus", AssetDescription = "Asus gaming laptop", AssetImage = "default-assets.png",AssetQRCodeImage = "default-qrcode.png", Status = "Available", CreatedDate = DateTime.UtcNow },
                new Asset { AssetName = "Mac", AssetDescription = "MacBook laptop", AssetImage = "default-assets.png",AssetQRCodeImage = "default-qrcode.png", Status ="Damaged", CreatedDate = DateTime.UtcNow },
                new Asset { AssetName = "Lenovo L16", AssetDescription = "Fast Lenovo Desktop", AssetImage = "default-assets.png",AssetQRCodeImage = "default-qrcode.png", Status ="Damaged", CreatedDate = DateTime.UtcNow },
                new Asset { AssetName = "L Table", AssetDescription = "Office table", AssetImage = "default-assets.png",AssetQRCodeImage = "default-qrcode.png", Status ="Damaged", CreatedDate = DateTime.UtcNow },
                new Asset { AssetName = "Cisco Router", AssetDescription = "Nice fast router", AssetImage = "default-assets.png",AssetQRCodeImage = "default-qrcode.png", Status ="Damaged", CreatedDate = DateTime.UtcNow },
                new Asset { AssetName = "TP Link Extender", AssetDescription = "Best Wi-Fi extender", AssetImage = "default-assets.png", CreatedDate = DateTime.UtcNow },
                new Asset { AssetName = "Cisco Firewall", AssetDescription = "Efficient firewall for network security", AssetImage = "default-assets.png",AssetQRCodeImage = "default-qrcode.png", Status="Available", CreatedDate = DateTime.UtcNow },
                new Asset { AssetName = "Cisco Swith", AssetDescription = "Efficient swith for network security", AssetImage = "default-assets.png",AssetQRCodeImage = "default-qrcode.png",RoomServerId=1,AssetCategoryServerId=5,Status="Available", CreatedDate = DateTime.UtcNow }
            };

            await context.Assets.AddRangeAsync(assets);
            await context.SaveChangesAsync();
            Console.WriteLine("Assets seeded.");
        }

        #endregion
    }
}
