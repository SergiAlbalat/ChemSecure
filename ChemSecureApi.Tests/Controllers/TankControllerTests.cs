using ChemSecureApi.Controllers;
using ChemSecureApi.Data;
using ChemSecureApi.DTOs;
using ChemSecureApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ChemSecureApi.Tests.Controllers
{
    public class TankControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public TankControllerTests()
        {
            // Configure the in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ChemSecureTestDb_" + Guid.NewGuid().ToString())
                .Options;
        }

        private AppDbContext CreateDbContext()
        {
            var context = new AppDbContext(_dbContextOptions);
            context.Database.EnsureCreated();
            return context;
        }

        private void SeedDatabase(AppDbContext context)
        {
            // Add test users
            var testUser = new User
            {
                Id = "user1",
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                Address = "123 Test Street"
            };

            var adminUser = new User
            {
                Id = "admin1",
                UserName = "admin@example.com",
                Email = "admin@example.com",
                Address = "456 Admin Street"
            };

            context.Users.Add(testUser);
            context.Users.Add(adminUser);

            // Add test tanks
            var tanks = new List<Tank>
            {
                new Tank
                {
                    Id = 1,
                    Capacity = 1000,
                    CurrentVolume = 500,
                    Type = residusType.Acids,
                    ClientId = "user1"
                },
                new Tank
                {
                    Id = 2,
                    Capacity = 2000,
                    CurrentVolume = 1000,
                    Type = residusType.AqueousSolutions,
                    ClientId = "user1"
                },
                new Tank
                {
                    Id = 3,
                    Capacity = 3000,
                    CurrentVolume = 1500,
                    Type = residusType.Oils,
                    ClientId = "admin1"
                }
            };

            context.Tanks.AddRange(tanks);
            context.SaveChanges();
        }

        private TankController CreateControllerWithUserIdentity(AppDbContext context, string userId, string role = null)
        {
            // Create claims for user authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "testuser@example.com")
            };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Create controller with HttpContext
            var controller = new TankController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            return controller;
        }

        [Fact]
        public async Task GetTanks_ReturnsAllTanks_WhenUserIsAdmin()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "admin1", "Admin");

            // Act
            var result = await controller.GetTanks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var tanks = Assert.IsAssignableFrom<List<TankGetDTO>>(okResult.Value);
            Assert.Equal(3, tanks.Count);
        }

        [Fact]
        public async Task GetTank_ReturnsNotFound_WhenTankDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "user1");
            var nonExistentTankId = 999;

            // Act
            var result = await controller.GetTank(nonExistentTankId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetTank_ReturnsTank_WhenTankExists()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "user1");
            var existingTankId = 1;

            // Act
            var result = await controller.GetTank(existingTankId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var tank = Assert.IsType<TankGetDTO>(okResult.Value);
            Assert.Equal(existingTankId, tank.Id);
            Assert.Equal(1000, tank.Capacity);
            Assert.Equal(500, tank.CurrentVolume);
            Assert.Equal(residusType.Acids, tank.Type);
        }

        [Fact]
        public async Task PostTank_CreatesNewTank_WhenModelIsValid()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "admin1", "Admin");
            var newTank = new TankInsertDTO
            {
                Id = 4,
                Capacity = 4000,
                CurrentVolume = 2000,
                Type = residusType.HalogenatedSolvents,
                ClientId = "user1"
            };

            // Act
            var result = await controller.PostTank(newTank);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<Tank>(createdAtActionResult.Value);
            Assert.Equal(newTank.Id, returnValue.Id);
            Assert.Equal(newTank.Capacity, returnValue.Capacity);
            Assert.Equal(newTank.CurrentVolume, returnValue.CurrentVolume);
            Assert.Equal(newTank.Type, returnValue.Type);

            // Verify tank was added to database
            Assert.Equal(4, context.Tanks.Count());
        }

        [Fact]
        public async Task DeleteTank_ReturnsNotFound_WhenTankDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "admin1", "Admin");
            var nonExistentTankId = 999;

            // Act
            var result = await controller.DeleteTank(nonExistentTankId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteTank_RemovesTank_WhenTankExists()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "admin1", "Admin");
            var existingTankId = 1;

            // Act
            var result = await controller.DeleteTank(existingTankId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify tank was removed from database
            Assert.Equal(2, context.Tanks.Count());
            Assert.Null(context.Tanks.Find(existingTankId));
        }

        [Fact]
        public async Task PutTank_ReturnsNotFound_WhenTankDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "admin1", "Admin");
            var nonExistentTankId = 999;
            var tankDto = new TankInsertDTO
            {
                Id = nonExistentTankId,
                Capacity = 5000,
                CurrentVolume = 2500,
                Type = residusType.Oils,
                ClientId = "user1"
            };

            // Act
            var result = await controller.PutTank(tankDto, nonExistentTankId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task PutTank_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "admin1", "Admin");
            var tankId = 1;
            var tankDto = new TankInsertDTO
            {
                Id = 2, // Mismatched ID
                Capacity = 5000,
                CurrentVolume = 2500,
                Type = residusType.Oils,
                ClientId = "user1"
            };

            // Act
            var result = await controller.PutTank(tankDto, tankId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task PutTank_UpdatesTank_WhenModelIsValid()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "admin1", "Admin");
            var tankId = 1;
            var tankDto = new TankInsertDTO
            {
                Id = tankId,
                Capacity = 5000,
                CurrentVolume = 2500,
                Type = residusType.Oils,
                ClientId = "user1"
            };

            // Act
            var result = await controller.PutTank(tankDto, tankId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify tank was updated in database
            var updatedTank = context.Tanks.Find(tankId);
            Assert.Equal(tankDto.Capacity, updatedTank.Capacity);
            Assert.Equal(tankDto.CurrentVolume, updatedTank.CurrentVolume);
            Assert.Equal(tankDto.Type, updatedTank.Type);
        }

        [Fact]
        public async Task UpdateTankVolume_ReturnsNotFound_WhenTankDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "user1");
            var nonExistentTankId = 999;
            var newVolume = 750.0;

            // Act
            var result = await controller.UpdateTankVolume(nonExistentTankId, newVolume);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTankVolume_ReturnsBadRequest_WhenVolumeExceedsCapacity()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "user1");
            var tankId = 1;
            var exceedingVolume = 2000.0; // Exceeds capacity of 1000

            // Act
            var result = await controller.UpdateTankVolume(tankId, exceedingVolume);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTankVolume_UpdatesVolume_WhenVolumeIsValid()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "user1");
            var tankId = 1;
            var newVolume = 750.0;

            // Act
            var result = await controller.UpdateTankVolume(tankId, newVolume);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify volume was updated in database
            var updatedTank = context.Tanks.Find(tankId);
            Assert.Equal(newVolume, updatedTank.CurrentVolume);
        }

        [Fact]
        public async Task GetUserTanks_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            
            // Create controller without user identity
            var controller = new TankController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.GetUserTanks();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUserTanks_ReturnsNotFound_WhenUserHasNoTanks()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            
            // Create a new user with no tanks
            var newUser = new User
            {
                Id = "user2",
                UserName = "newuser@example.com",
                Email = "newuser@example.com",
                Address = "789 New User Street"
            };
            context.Users.Add(newUser);
            context.SaveChanges();
            
            var controller = CreateControllerWithUserIdentity(context, "user2");

            // Act
            var result = await controller.GetUserTanks();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUserTanks_ReturnsUserTanks_WhenUserHasTanks()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "user1");

            // Act
            var result = await controller.GetUserTanks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var tanks = Assert.IsAssignableFrom<List<TankGetDTO>>(okResult.Value);
            Assert.Equal(2, tanks.Count); // user1 has 2 tanks in the seed data
            Assert.All(tanks, tank => Assert.Contains(tank.Id, new[] { 1, 2 }));
        }
    }
}
