using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicTest.Services
{
    public class OAuthServiceTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IJwt> _jwtMock;
        private readonly OAuthService _oAuthService;

        public OAuthServiceTest()
        {
            var store = new Mock<IUserStore<User>>();

            _userManagerMock = new Mock<UserManager<User>>(
                store.Object,
                null!, null!, null!, null!, null!, null!, null!, null!
            );

            _jwtMock = new Mock<IJwt>();

            _oAuthService = new OAuthService(_userManagerMock.Object, _jwtMock.Object);
        }

        #region HandleExternalLogin

        [Fact]
        public async Task HandleExternalLogin_ShouldReturnToken_WhenUserExists()
        {
            //Arrange
            var email = "test@test.com";
            var fullName = "Test User";

            var user = new User { Id = "user-1", Email = email, UserName = email, Name = fullName, Nickname = "test", DateOfBirth = DateTime.Now, Gender = Gender.Male, PhotoUrl = "" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _jwtMock
                .Setup(x => x.GenerateToken(user))
                .ReturnsAsync("fake-token");

            //Act
            var result = await _oAuthService.HandleExternalLoginAsync(email, fullName);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("fake-token");
        }

        [Fact]
        public async Task HandleExternalLogin_ShouldCreateUserAndReturnToken_WhenUserNotExists()
        {
            //Arrange
            var email = "new@test.com";
            var fullName = "New User";

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((User)null!);
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.User.ToString()))
                .ReturnsAsync(IdentityResult.Success);
            _jwtMock
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .ReturnsAsync("new-token");

            //Act
            var result = await _oAuthService.HandleExternalLoginAsync(email, fullName);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("new-token");
        }

        [Fact]
        public async Task HandleExternalLogin_ShouldReturnFail_WhenUserCreationFails()
        {
            //Arrange
            var email = "new@test.com";
            var fullName = "New User";

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((User)null!);
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                    {
                        Description = "Creation failed"   
                    }));

            //Act
            var result = await _oAuthService.HandleExternalLoginAsync(email, fullName);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("Creation failed");
        }

        [Fact]
        public async Task HandleExternalLogin_ShouldReturnFail_WhenAddingRoleFails()
        {
            //Arrange
            var email = "new@test.com";
            var fullName = "New User";

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((User)null!);
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.User.ToString()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Description = "Failed to add a role"
                }));

            //Act
            var result = await _oAuthService.HandleExternalLoginAsync(email, fullName);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("Failed to add a role");
        }

        #endregion
    }
}
