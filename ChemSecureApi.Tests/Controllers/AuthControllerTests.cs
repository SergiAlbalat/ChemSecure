using ChemSecureApi.Controllers;
using ChemSecureApi.Data;
using ChemSecureApi.DTOs;
using ChemSecureApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ChemSecureApi.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockConfigSection;
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public AuthControllerTests()
        {
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Setup Configuration mock
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfigSection = new Mock<IConfigurationSection>();

            _mockConfigSection.Setup(s => s["Key"]).Returns("ThisIsASecretKeyForTestingPurposesOnly12345");
            _mockConfigSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
            _mockConfigSection.Setup(s => s["Audience"]).Returns("TestAudience");
            _mockConfigSection.Setup(s => s["ExpirationMinutes"]).Returns("60");

            _mockConfiguration.Setup(c => c.GetSection("JwtSettings")).Returns(_mockConfigSection.Object);

            // Configure in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ChemSecureAuthTestDb_" + Guid.NewGuid().ToString())
                .Options;
        }

        private AppDbContext CreateDbContext()
        {
            var context = new AppDbContext(_dbContextOptions);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
        {
            // Arrange
            var context = CreateDbContext();
            var registerDto = new RegisterDTO
            {
                Email = "test@example.com",
                Password = "Password123!",
                Name = "Test User",
                Phone = "123456789",
                Address = "123 Test St"
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new AuthController(_mockUserManager.Object, _mockConfiguration.Object, context);

            // Act
            var result = await controller.Register(registerDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var context = CreateDbContext();
            var registerDto = new RegisterDTO
            {
                Email = "test@example.com",
                Password = "weak",
                Name = "Test User",
                Phone = "123456789",
                Address = "123 Test St"
            };

            var errors = new List<IdentityError> { new IdentityError { Description = "Password is too weak" } };
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

            var controller = new AuthController(_mockUserManager.Object, _mockConfiguration.Object, context);

            // Act
            var result = await controller.Register(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            var context = CreateDbContext();
            var loginDto = new LoginDTO
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((User)null);

            var controller = new AuthController(_mockUserManager.Object, _mockConfiguration.Object, context);

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordIncorrect()
        {
            // Arrange
            var context = CreateDbContext();
            var user = new User
            {
                Id = "testuser1",
                UserName = "Test User",
                Email = "test@example.com"
            };

            var loginDto = new LoginDTO
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(false);

            var controller = new AuthController(_mockUserManager.Object, _mockConfiguration.Object, context);

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsOkWithToken_WhenCredentialsValid()
        {
            // Arrange
            var context = CreateDbContext();
            var user = new User
            {
                Id = "testuser1",
                UserName = "Test User",
                Email = "test@example.com"
            };

            var loginDto = new LoginDTO
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            var controller = new AuthController(_mockUserManager.Object, _mockConfiguration.Object, context);

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.IsType<string>(okResult.Value);
        }

        [Fact]
        public async Task RegisterAdmin_ReturnsOk_WhenRegistrationSucceeds()
        {
            // Arrange
            var context = CreateDbContext();
            var registerDto = new RegisterDTO
            {
                Email = "admin@example.com",
                Password = "AdminPass123!",
                Name = "Admin User",
                Phone = "987654321",
                Address = "456 Admin St"
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new AuthController(_mockUserManager.Object, _mockConfiguration.Object, context);

            // Act
            var result = await controller.RegisterAdmin(registerDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task RegisterAdmin_ReturnsBadRequest_WhenRoleAssignmentFails()
        {
            // Arrange
            var context = CreateDbContext();
            var registerDto = new RegisterDTO
            {
                Email = "admin@example.com",
                Password = "AdminPass123!",
                Name = "Admin User",
                Phone = "987654321",
                Address = "456 Admin St"
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            var errors = new List<IdentityError> { new IdentityError { Description = "Role does not exist" } };
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Admin"))
                .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

            var controller = new AuthController(_mockUserManager.Object, _mockConfiguration.Object, context);

            // Act
            var result = await controller.RegisterAdmin(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RegisterManager_ReturnsOk_WhenRegistrationSucceeds()
        {
            // Arrange
            var context = CreateDbContext();
            var registerDto = new RegisterDTO
            {
                Email = "manager@example.com",
                Password = "ManagerPass123!",
                Name = "Manager User",
                Phone = "555666777",
                Address = "789 Manager St"
            };

            // Setup claims for admin authorization
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "adminId"),
                new Claim(ClaimTypes.Name, "admin@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Manager"))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new AuthController(_mockUserManager.Object, _mockConfiguration.Object, context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            // Act
            var result = await controller.RegisterManager(registerDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
