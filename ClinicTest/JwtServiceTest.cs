using Application.Interfaces;
//using Castle.Core.Configuration;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ClinicTest.Services
{
    public class JwtServiceTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly IJwt _jwtService;

        public JwtServiceTest()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object,
                null, null, null, null, null, null, null, null
            );

            _configMock = new Mock<IConfiguration>();

            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x["Key"]).Returns("THIS_IS_A_SUPER_SECRET_KEY_FOR_UNIT_TESTS_123456");
            configSectionMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
            configSectionMock.Setup(x => x["Audience"]).Returns("TestAudience");
            configSectionMock.Setup(x => x["DurationInMinutes"]).Returns("60");

            _configMock.Setup(x => x.GetSection("Jwt"))
                .Returns(configSectionMock.Object);

            _jwtService = new Jwt(_userManagerMock.Object, _configMock.Object);
        }

        #region GenerateToken

        [Fact]
        public async Task GenerateToken_ShouldReturnToken_WhenUserValid()
        {
            // Arrange
            var user = new User
            {
                Name = "test name",
                Nickname = "Test Name",
                Id = "user-1",
                UserName = "testuser",
                Email = "test@test.com"
            };

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var token = await _jwtService.GenerateToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GenerateToken_ShouldIncludeUserClaims()
        {
            // Arrange
            var user = new User
            {
                Name = "test name",
                Nickname = "Test Name",
                Id = "user-1",
                UserName = "testuser",
                Email = "test@test.com"
            };

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            var token = await _jwtService.GenerateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        }

        [Fact]
        public async Task GenerateToken_ShouldIncludeRoles()
        {
            // Arrange
            var user = new User
            {
                Name = "test name",
                Nickname = "Test Name",
                Id = "user-1",
                UserName = "testuser",
                Email = "test@test.com"
            };

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin", "User" });

            var token = await _jwtService.GenerateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        }

        #endregion
    }
}
