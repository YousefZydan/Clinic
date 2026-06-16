using Application.AutoMapper;
using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicTest.Services
{
    public class AccountServiceTest
    {
        ApplicationDbContext _dbContext;
        IMapper _mapper;
        Mock<UserManager<User>> _userManagerMock;
        Mock<ICloudinaryService> _photoMock;
        Mock<IJwt> _jwtMock;
        Mock<IEmailService> _emailMock;
        AccountService _accountService;

        public AccountServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _dbContext = new ApplicationDbContext(options, httpContextAccessorMock.Object);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
            var provider = services.BuildServiceProvider();
            _mapper = provider.GetRequiredService<IMapper>();

            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object,
                null, null, null, null, null, null, null, null
            );

            _photoMock = new Mock<ICloudinaryService>();
            _jwtMock = new Mock<IJwt>();
            _emailMock = new Mock<IEmailService>();

            _accountService = new AccountService(
                _userManagerMock.Object,
                _photoMock.Object,
                _jwtMock.Object,
                _emailMock.Object,
                _dbContext,
                _mapper
            );
        }

        #region Register

        [Fact]
        public async Task Register_ShouldReturnSuccess_WhenUserCreated()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Name = "Test User",
                Nickname = "tester",
                Email = "test@test.com",
                UserName = "testuser",
                Password = "123456",
                ConfirmPassword = "123456",
                DateOfBirth = DateTime.Now.AddYears(-20),
                Gender = Gender.Male,
                Phone = "123456789",
                Photo = null
            };
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);
            _photoMock
                .Setup(x => x.AddPhotoAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(new ImageUploadResult
                {
                    Url = new Uri("http://photo.com/img.jpg"),
                    PublicId = "123"
                });
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.User.ToString()))
                .ReturnsAsync(IdentityResult.Success);
            _jwtMock
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .ReturnsAsync("fake-token");

            // Act
            var result = await _accountService.Register(dto);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Email.Should().Be(dto.Email);
            result.Data.Token.Should().Be("fake-token");

        }

        [Fact]
        public async Task Register_ShouldReturnFail_WhenEmailAlreadyExists()
        {
            // Arrange
            var dto = new RegisterDto { Email = "test@test.com", UserName = "testuser", Password = "123456", ConfirmPassword = "123456", Name = "Test", Nickname = "test", Phone = "123", DateOfBirth = DateTime.Now.AddYears(-20) };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(new User { Name = "Test User", Nickname = "tester", Email = "test@test.com", UserName = "testuser" });

            // Act
            var result = await _accountService.Register(dto);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Email already exists");
        }

        [Fact]
        public async Task Register_ShouldReturnFail_WhenUserCreationFails()
        {
            // Arrange
            var dto = new RegisterDto { Email = "test@test.com", UserName = "testuser", Password = "123456", ConfirmPassword = "123456", Name = "Test", Nickname = "test", Phone = "123", DateOfBirth = DateTime.Now.AddYears(-20) };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Description = "Create failed"
                }));

            // Act
            var result = await _accountService.Register(dto);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Create failed");
        }

        [Fact]
        public async Task Register_ShouldReturnFail_WhenAddingRoleFails()
        {
            // Arrange
            var dto = new RegisterDto { Email = "test@test.com", UserName = "testuser", Password = "123456", ConfirmPassword = "123456", Name = "Test", Nickname = "test", Phone = "123", DateOfBirth = DateTime.Now.AddYears(-20) };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.User.ToString()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Description = "Role failed"
                }));

            // Act
            var result = await _accountService.Register(dto);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Failed to assign role: Role failed");
        }

        [Fact]
        public async Task Register_ShouldUploadPhoto_WhenPhotoProvided()
        {
            var dto = new RegisterDto
            {
                Name = "Test User",
                Nickname = "tester",
                Email = "test@test.com",
                UserName = "testuser",
                Password = "123456",
                ConfirmPassword = "123456",
                DateOfBirth = DateTime.Now.AddYears(-20),
                Gender = Gender.Male,
                Phone = "123456789",
                Photo = Mock.Of<IFormFile>()
            };

            _photoMock
                .Setup(x => x.AddPhotoAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(new ImageUploadResult
                {
                    Url = new Uri("http://photo.com"),
                    PublicId = "123"
                });
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.User.ToString()))
                .ReturnsAsync(IdentityResult.Success);
            _jwtMock
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .ReturnsAsync("Token");

            //Act
            var result = await _accountService.Register(dto);

            //Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Email.Should().Be(dto.Email);
            result.Data.Token.Should().Be("Token");
        }

        #endregion

        #region Login

        [Fact]
        public async Task Login_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            //Arrange
            var dto = new LoginDto { Email = "test@test.com", Password = "123456" };

            var user = new User { Name = "Test User", Nickname = "tester", Email = dto.Email, UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>
                {
                    Roles.User.ToString()
                });
            _jwtMock
                .Setup(x => x.GenerateToken(user))
                .ReturnsAsync("fake-token");

            // Act
            var result = await _accountService.Login(dto);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Email.Should().Be(dto.Email);
            result.Data.UserName.Should().Be(user.UserName);
            result.Data.Token.Should().Be("fake-token");
        }

        [Fact]
        public async Task Login_ShouldReturnFail_WhenEmailNotFound()
        {
            //Arrange
            var dto = new LoginDto { Email = "test@test.com", Password = "123456" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            //Act
            var result = await _accountService.Login(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Email or password is incorrect!");
        }

        [Fact]
        public async Task Login_ShouldReturnFail_WhenPasswordIsInvalid()
        {
            //Arrange
            var dto = new LoginDto { Email = "test@test.com", Password = "123456" };

            var user = new User { Name = "test user", Nickname = "tester", Email = dto.Email, UserName = "testuser" };
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(false);

            //Act
            var result = await _accountService.Login(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Email or password is incorrect!");
        }

        [Fact]
        public async Task Login_ShouldAddRole_WhenUserHasNoUesrRole()
        {
            //Arrange
            var dto = new LoginDto { Email = "test@test.com", Password = "123456" };

            var user = new User { Name = "Test User", Nickname = "tester", Email = dto.Email, UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock
                .Setup(x => x.AddToRoleAsync(user, Roles.User.ToString()))
                .ReturnsAsync(IdentityResult.Success);
            _jwtMock
                .Setup(x => x.GenerateToken(user))
                .ReturnsAsync("fake-token");

            //Act
            var result = await _accountService.Login(dto);

            //Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Token.Should().Be("fake-token");
        }

        [Fact]
        public async Task Login_ShouldReturnFail_WhenAddingRoleFails()
        {
            //Arrange
            var dto = new LoginDto { Email = "test@test.com", Password = "123456" };

            var user = new User { Name = "Test User", Nickname = "tester", Email = dto.Email, UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock
                .Setup(x => x.AddToRoleAsync(user, Roles.User.ToString()))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Role failed"
                    }));

            //Act
            var result = await _accountService.Login(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("Role failed");
        }

        #endregion

        #region UpdateUserRoles

        [Fact]
        public async Task UpdateUserRolesAsync_ShouldReturnFail_WhenUserNotFound()
        {
            //Arrange
            var dto = new UpdateRolesDto { Roles = new List<string> { "Admin" }, UserId = "user-id" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(dto.UserId))
                .ReturnsAsync((User?)null);

            //Act
            var result = await _accountService.UpdateUserRolesAsync(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("User not found");
        }

        [Fact]
        public async Task UpdateUserRolesAsync_ShouldReturnFail_WhenRemoveRolesFails()
        {
            //Arrange
            var dto = new UpdateRolesDto { UserId = "user-id", Roles = new List<string> { "Admin" } };
            
            var user = new User { Name = "Test User", Nickname = "tester", Email = "test@test.com", UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(dto.UserId))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User", "Doctor" });
            _userManagerMock
                .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Remove roles failed"
                    }));

            //Act
            var result = await _accountService.UpdateUserRolesAsync(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("Remove roles failed");
        }

        [Fact]
        public async Task UpdateUserRolesAsync_ShouldReturnFail_WhenAddRolesFails()
        {
            //Arrange
            var dto = new UpdateRolesDto { UserId = "user-id", Roles = new List<string> { "Admin" } };

            var user = new User { Name = "Test User", Nickname = "tester", Email = "test@test.com", UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(dto.UserId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            _userManagerMock
                .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRolesAsync(user, dto.Roles))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Add roles failed"
                    }));

            // Act
            var result = await _accountService.UpdateUserRolesAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("Add roles failed");
        }

        [Fact]
        public async Task UpdateUserRolesAsync_ShouldReturnSuccess_WhenRolesUpdated()
        {
            //Arrange
            var dto = new UpdateRolesDto { UserId = "user-id", Roles = new List<string> { "Admin" } };

            var user = new User { Name = "Test User", Nickname = "tester", Email = "test@test.com", UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(dto.UserId))
                .ReturnsAsync(user);

            _userManagerMock
                .SetupSequence(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" })
                .ReturnsAsync(dto.Roles);

            _userManagerMock
                .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRolesAsync(user, dto.Roles))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.UpdateUserRolesAsync(dto);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().ContainSingle();
            result.Data.Should().Contain("Admin");
        }

        [Fact]
        public async Task UpdateUserRolesAsync_ShouldReturnSuccess_WhenUserHasNoCurrentRoles()
        {
            //Arrange
            var dto = new UpdateRolesDto { UserId = "user-id", Roles = new List<string> { "Admin" } };

            var user = new User { Name = "Test User", Nickname = "tester", Email = "test@test.com", UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(dto.UserId))
                .ReturnsAsync(user);

            _userManagerMock
                .SetupSequence(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>())
                .ReturnsAsync(dto.Roles);

            _userManagerMock
                .Setup(x => x.AddToRolesAsync(user, dto.Roles))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.UpdateUserRolesAsync(dto);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Contain("Admin");

            _userManagerMock.Verify(
                x => x.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()),
                Times.Never);
        }

        #endregion

        #region VerifyOtp

        [Fact]
        public async Task VerifyOtpAsync_ShouldReturnTrue_WhenOtpIsValid()
        {
            // Arrange
            var dto = new VerifyOtpDto { Email = "test@test.com", Code = "123456" };

            _dbContext.ResetCodes.Add(new ResetCode
            {
                Email = dto.Email,
                Code = dto.Code,
                ExpireAt = DateTime.UtcNow.AddMinutes(10)
            });

            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _accountService.VerifyOtpAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task VerifyOtpAsync_ShouldReturnFalse_WhenOtpNotFound()
        {
            // Arrange
            var dto = new VerifyOtpDto { Email = "test@test.com", Code = "999999" };

            // Act
            var result = await _accountService.VerifyOtpAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task VerifyOtpAsync_ShouldReturnTrue_WhenOtpExpired()
        {
            // Arrange
            var dto = new VerifyOtpDto { Email = "test@test.com", Code = "123456" };

            _dbContext.ResetCodes.Add(new ResetCode
            {
                Email = dto.Email,
                Code = dto.Code,
                ExpireAt = DateTime.UtcNow.AddMinutes(-10)
            });

            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _accountService.VerifyOtpAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeFalse();
        }

        #endregion

        #region ForgotPassword

        [Fact]
        public async Task ForgotPassword_ShouldReturnFail_WhenEmailNotFound()
        {
            //Arrange
            var dto = new ForgotPasswordDto { Email = "notfound@test.com" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            //Act
            var result = await _accountService.ForgotPassword(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Email not found");
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnSuccess_WhenEmailExists()
        {
            //Arrange
            var dto = new ForgotPasswordDto { Email = "test@test.com" };

            var user = new User { Name = "Test User", Nickname = "tester", Email = dto.Email, UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _emailMock
                .Setup(x => x.SendEmailAsync(
                    dto.Email,
                    "Password Reset Code",
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            //Act
            var result = await _accountService.ForgotPassword(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("OTP sent to your email");
        }
        #endregion

        #region ResetPassword

        [Fact]
        public async Task ResetPassword_ShouldReturnFail_WhenPasswordsDoNotMatch()
        {
            //Arrange
            var dto = new ResetPasswordDto { Email = "test@test.com", Code = "123456", NewPassword = "123456", ConfirmPassword = "different" };

            //Act
            var result = await _accountService.ResetPassword(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Passwords do not match");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnFail_WhenUserNotFound()
        {
            //Arrange
            var dto = new ResetPasswordDto { Email = "test@test.com", Code = "123456", NewPassword = "123456", ConfirmPassword = "123456" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            //Act
            var result = await _accountService.ResetPassword(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Email not found");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnFail_WhenOtpNotFound()
        {
            //Arrange
            var dto = new ResetPasswordDto { Email = "test@test.com", Code = "123456", NewPassword = "123456", ConfirmPassword = "123456" };

            var user = new User { Name = "Test User", Nickname = "tester", Email = dto.Email, UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            //Act
            var result = await _accountService.ResetPassword(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("OTP not verified or expired");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnFail_WhenOtpExpired()
        {
            //Arrange
            var dto = new ResetPasswordDto { Email = "test@test.com", Code = "123456", NewPassword = "123456", ConfirmPassword = "123456" };

            var user = new User { Name = "Test User", Nickname = "tester", Email = dto.Email, UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _dbContext.ResetCodes.Add(new ResetCode
            {
                Email = dto.Email,
                Code = dto.Code,
                ExpireAt = DateTime.UtcNow.AddMinutes(-10)
            });

            await _dbContext.SaveChangesAsync();

            //Act
            var result = await _accountService.ResetPassword(dto);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("OTP not verified or expired");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnSuccess_WhenValidRequest()
        {
            //Arrange
            var dto = new ResetPasswordDto { Email = "test@test.com", Code = "123456", NewPassword = "123456", ConfirmPassword = "123456" };

            var user = new User { Name = "Test User", Nickname = "tester", Email = dto.Email, UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _dbContext.ResetCodes.Add(new ResetCode
            {
                Email = dto.Email,
                Code = dto.Code,
                ExpireAt = DateTime.UtcNow.AddMinutes(10)
            });

            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(x => x.RemovePasswordAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddPasswordAsync(user, dto.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.ResetPassword(dto);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        #endregion

        #region GetProfileInfo

        [Fact]
        public async Task GetProfileInfo_ShouldReturnNull_WhenUserNotFound()
        {
            //Arrange
            var userId = "user-id";

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User?)null);

            //Act
            var result = await _accountService.GetProfileInfo(userId);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProfileInfo_ShouldReturnUserDto_WhenUserExists()
        {
            //Arrange
            var userId = "user-id";

            var user = new User { Name = "Test User", Nickname = "tester", Email = "test@test.com", UserName = "testuser", PhoneNumber = "123456", PhotoUrl = "photo.jpg", DateOfBirth = DateTime.Now };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            //Act
            var result = await _accountService.GetProfileInfo(userId);

            //Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(user.Name);
            result.Nickname.Should().Be(user.Nickname);
            result.Email.Should().Be(user.Email);
            result.UserName.Should().Be(user.UserName);
        }

        #endregion

        #region EditProfile

        [Fact]
        public async Task EditProfile_ShouldReturnFail_WhenUserNotFound()
        {
            //Arrange
            var currentUser = new User { Name = "Current User", Id = "user-1", Nickname = "current", Email = "current@test.com", UserName = "currentuser" };

            var dto = new EditProfileDto { Email = currentUser.Email, UserName = "newusername", Name = "New Name", Nickname = "New Nick", PhoneNumber = "01009752770" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(currentUser.Id))
                .ReturnsAsync((User?)null);

            //Act
            var result = await _accountService.EditProfile(dto, currentUser.Id);

            //Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("User not found");
        }

        [Fact]
        public async Task EditProfile_ShouldReturnFail_WhenEmailAlreadyExists()
        {
            //Arrange
            var userId = "user-1";

            var existingUser = new User { Id = "another-user", Name = "Existing", Nickname = "existing", Email = "existing@test.com", UserName = "existinguser" };

            var dto = new EditProfileDto { Email = existingUser.Email, UserName = "newusername", Name = "New Name", Nickname = "New Nick", PhoneNumber = "01009752770" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(existingUser);

            //Act
            var result = await _accountService.EditProfile(dto, userId);

            //Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Email already exists");
        }

        [Fact]
        public async Task EditProfile_ShouldReturnFail_WhenUserNameAlreadyExists()
        {
            //Arrange
            var userId = "user-1";

            var existingUser = new User { Id = "another-user", Name = "Existing", Nickname = "existing", Email = "existing@test.com", UserName = "existinguser" };

            var dto = new EditProfileDto { Email = "new@test.com", UserName = existingUser.UserName, Name = "New Name", Nickname = "New Nick", PhoneNumber = "01009752770" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);
            _userManagerMock
                .Setup(x => x.FindByNameAsync(dto.UserName))
                .ReturnsAsync(existingUser);

            //Act
            var result = await _accountService.EditProfile(dto, userId);

            //Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Username already exists");
        }

        [Fact]
        public async Task EditProfile_ShouldReturnSuccess_WhenValidWithoutPhoto()
        {
            // Arrange
            var userId = "user-1";

            var user = new User { Id = userId, Name = "Old Name", Nickname = "Old Nick", Email = "old@test.com", UserName = "olduser" };

            var dto = new EditProfileDto { Name = "New Name", Nickname = "New Nick", Email = "new@test.com", UserName = "newuser", PhoneNumber = "123456" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            _userManagerMock
                .Setup(x => x.FindByNameAsync(dto.UserName))
                .ReturnsAsync((User?)null);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.EditProfile(dto, userId);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("Profile updated successfully");

            user.Name.Should().Be(dto.Name);
            user.Nickname.Should().Be(dto.Nickname);
            user.Email.Should().Be(dto.Email);
            user.UserName.Should().Be(dto.UserName);
        }

        [Fact]
        public async Task EditProfile_ShouldReturnFail_WhenUpdateFails()
        {
            // Arrange
            var userId = "user-1";

            var user = new User { Id = userId, Name = "Old Name", Nickname = "Old Nick", Email = "old@test.com", UserName = "olduser" };

            var dto = new EditProfileDto { Name = "New Name", Nickname = "New Nick", Email = "new@test.com", UserName = "newuser", PhoneNumber = "123456" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            _userManagerMock
                .Setup(x => x.FindByNameAsync(dto.UserName))
                .ReturnsAsync((User?)null);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(
                    IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = "Update failed"
                        }));

            // Act
            var result = await _accountService.EditProfile(dto, userId);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("Update failed");
        }



        #endregion

        #region GetRolesOfUser

        [Fact]
        public async Task GetRolesOfUser_ShouldReturnFail_WhenUserNotFound()
        {
            // Arrange
            var userId = "user-1";

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _accountService.GetRolesOfUser(userId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("user not found");
        }

        [Fact]
        public async Task GetRolesOfUser_ShouldReturnEmptyList_WhenUserHasNoRoles()
        {
            // Arrange
            var userId = "user-1";

            var user = new User { Id = userId, Name = "Test User", Nickname = "tester", Email = "test@test.com", UserName = "testuser" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _accountService.GetRolesOfUser(userId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRolesOfUser_ShouldReturnRoles_WhenUserExists()
        {
            // Arrange
            var userId = "user-1";

            var user = new User { Id = userId, Name = "Test User", Nickname = "tester", Email = "test@test.com", UserName = "testuser" };

            var roles = new List<string> { "User", "Admin" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            // Act
            var result = await _accountService.GetRolesOfUser(userId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(roles);
        }

        #endregion

        #region GetPrescriptionByDoctorId

        [Fact]
        public async Task GetPrescriptionByDoctorId_ShouldReturnFail_WhenDoctorNotFound()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var userId = "user-1";

            // Act
            var result = await _accountService.GetPrescriptionByDoctorId(doctorId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Doctor not found");
        }

        [Fact]
        public async Task GetPrescriptionByDoctorId_ShouldReturnEmptyList_WhenNoPrescriptionsExist()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var userId = "user-1";

            var doctor = new Doctor(
                "Doctor 1",
                "About",
                "owner",
                Guid.NewGuid());

            typeof(Doctor)
                .GetProperty(nameof(Doctor.Id))!
                .SetValue(doctor, doctorId);

            _dbContext.Doctors.Add(doctor);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _accountService.GetPrescriptionByDoctorId(
                doctorId,
                userId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPrescriptionByDoctorId_ShouldReturnPrescriptions_WhenDataExists()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var userId = "user-1";

            var category = new Category("Cardiology");

            var doctor = new Doctor( "Ahmed", "About", "owner", category.Id);

            typeof(Doctor)
                .GetProperty(nameof(Doctor.Id))!
                .SetValue(doctor, doctorId);

            typeof(Doctor)
                .GetProperty(nameof(Doctor.Category))!
                .SetValue(doctor, category);

            var prescription = (Prescription)Activator
                .CreateInstance(typeof(Prescription), true)!;
            prescription.DoctorId = doctorId;
            prescription.UserId = userId;
            prescription.PrescriptionUrl = "test-url";
            prescription.Doctor = doctor;

            _dbContext.Categories.Add(category);
            _dbContext.Doctors.Add(doctor);
            _dbContext.Prescriptions.Add(prescription);

            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _accountService.GetPrescriptionByDoctorId(doctorId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue(); 
            result.Data.Should().HaveCount(1); 
            result.Data![0].DoctorName.Should().Be("Ahmed");
            result.Data[0].DoctorCategory.Should().Be("Cardiology");
            result.Data[0].PrescriptionUrl.Should().Be("test-url");
        }

        #endregion
    }
}
