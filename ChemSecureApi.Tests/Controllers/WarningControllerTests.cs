using ChemSecureApi.Controllers;
using ChemSecureApi.Data;
using ChemSecureApi.DTOs;
using ChemSecureApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ChemSecureApi.Tests.Controllers
{
    public class WarningControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public WarningControllerTests()
        {
            // Configure in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ChemSecureWarningTestDb_" + Guid.NewGuid().ToString())
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

            var managerUser = new User
            {
                Id = "manager1",
                UserName = "manager@example.com",
                Email = "manager@example.com",
                Address = "789 Manager Avenue"
            };

            context.Users.Add(testUser);
            context.Users.Add(managerUser);

            // Add test tanks
            var tank = new Tank
            {
                Id = 1,
                Capacity = 1000,
                CurrentVolume = 800,
                Type = residusType.Acids,
                ClientId = "user1"
            };

            context.Tanks.Add(tank);

            // Add test warnings
            var warnings = new List<Warning>
            {
                new Warning
                {
                    Id = 1,
                    ClientName = "Test User",
                    Capacity = 1000,
                    CurrentVolume = 800,
                    CreationDate = DateTime.UtcNow.AddDays(-1),
                    TankId = 1,
                    Type = residusType.Acids
                },
                new Warning
                {
                    Id = 2,
                    ClientName = "Test User",
                    Capacity = 1000,
                    CurrentVolume = 900,
                    CreationDate = DateTime.UtcNow,
                    TankId = 1,
                    Type = residusType.Acids
                }
            };

            context.Warnings.AddRange(warnings);
            context.SaveChanges();
        }

        private WarningController CreateControllerWithUserIdentity(AppDbContext context, string userId, string role = null)
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
            var controller = new WarningController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            return controller;
        }

        [Fact]
        public async Task GetWarnings_ReturnsAllWarnings_WhenUserIsManager()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "manager1", "Manager");

            // Act
            var result = await controller.GetWarnings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var warnings = Assert.IsAssignableFrom<List<Warning>>(okResult.Value);
            Assert.Equal(2, warnings.Count);
        }

        [Fact]
        public async Task GetWarnings_ReturnsAllWarnings_WhenUserIsAdmin()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "admin1", "Admin");

            // Act
            var result = await controller.GetWarnings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var warnings = Assert.IsAssignableFrom<List<Warning>>(okResult.Value);
            Assert.Equal(2, warnings.Count);
        }

        [Fact]
        public async Task GetWarnings_ReturnsNotFound_WhenNoWarningsExist()
        {
            // Arrange
            using var context = CreateDbContext();
            // Don't seed the database with warnings
            var controller = CreateControllerWithUserIdentity(context, "manager1", "Manager");

            // Act
            var result = await controller.GetWarnings();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task AddWarning_ReturnsBadRequest_WhenWarningDtoIsNull()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "user1");

            // Act
            var result = await controller.AddWarning(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddWarning_CreatesWarning_WhenModelIsValid()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "user1");
            var warningDto = new WarningDTO
            {
                ClientName = "New Warning User",
                Capacity = 1000,
                CurrentVolume = 950,
                TankId = 1,
                Type = residusType.Acids
            };

            // Act
            var result = await controller.AddWarning(warningDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Warning>(createdAtActionResult.Value);
            Assert.Equal(warningDto.ClientName, returnValue.ClientName);
            Assert.Equal(warningDto.Capacity, returnValue.Capacity);
            Assert.Equal(warningDto.CurrentVolume, returnValue.CurrentVolume);
            Assert.Equal(warningDto.TankId, returnValue.TankId);
            Assert.Equal(warningDto.Type, returnValue.Type);

            // Verify warning was added to database
            Assert.Equal(3, context.Warnings.Count());
        }

        [Fact]
        public async Task AddWarning_SetsCreationDateToUtcNow()
        {
            // Arrange
            using var context = CreateDbContext();
            SeedDatabase(context);
            var controller = CreateControllerWithUserIdentity(context, "user1");
            var warningDto = new WarningDTO
            {
                ClientName = "Date Test User",
                Capacity = 1000,
                CurrentVolume = 950,
                TankId = 1,
                Type = residusType.Acids
            };

            var beforeTest = DateTime.UtcNow.AddSeconds(-1); // Allow 1 second buffer

            // Act
            var result = await controller.AddWarning(warningDto);

            var afterTest = DateTime.UtcNow.AddSeconds(1); // Allow 1 second buffer

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Warning>(createdAtActionResult.Value);
            
            // Verify the creation date is set to current UTC time
            Assert.True(returnValue.CreationDate >= beforeTest);
            Assert.True(returnValue.CreationDate <= afterTest);
        }
    }
}
