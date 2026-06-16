using Application.AutoMapper;
using Application.Dtos;
using Application.Helpers;
using Application.Repository;
using AutoFixture;
using AutoMapper;
using Domain.Entities;
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
    public class FavouriteServiceTests
    {
        ApplicationDbContext _dbContext;
        IMapper _mapper;
        Mock<IGenericRepository<Favourite>> _repoMock;
        Mock<UserManager<User>> _userManagerMock;
        FavouriteService _favouriteService;
        //Fixture _fixture;

        public FavouriteServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _dbContext = new ApplicationDbContext(
                options,
                httpContextAccessorMock.Object
            );

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
            var provider = services.BuildServiceProvider();
            _mapper = provider.GetRequiredService<IMapper>();

            _repoMock = new Mock<IGenericRepository<Favourite>>();

            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            _favouriteService = new FavouriteService(_mapper, _repoMock.Object, _userManagerMock.Object, _dbContext);
        }

        #region AddToFavourite

        [Fact]
        public async Task AddToFavourite_ShouldReturnFail_WhenUserNotFound()
        {
            // Arrange
            var input = new FavouriteCreateDto(Guid.NewGuid());
            _userManagerMock.Setup(t => t.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _favouriteService.AddToFav(input, "");

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("User not found");
        }

        [Fact]
        public async Task AddToFavourite_ShouldReturnFail_WhenDoctorAlreadyInFavourites()
        {
            // Arrange
            var category = new Category("Cardiology");
            _dbContext.Categories.Add(category);

            var doctor = new Doctor("Dr Ahmed", "Heart specialist", "user-1", category.Id);
            _dbContext.Doctors.Add(doctor);

            var user = new User { Id = "user-1", UserName = "user1", Email = "user1@test.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) };

            _userManagerMock.Setup(t => t.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var favourite = new Favourite(user.Id, doctor.Id);
            _dbContext.Favourites.Add(favourite);
            await _dbContext.SaveChangesAsync();

            var input = new FavouriteCreateDto(doctor.Id);

            var result = await _favouriteService.AddToFav(input, user.Id);

            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Doctor already in favourites");
        }

        [Fact]
        public async Task AddToFavourite_ShouldReturnSuccess_WhenDoctorNotInFavourites()
        {
            // Arrange
            var category = new Category("Cardiology");
            _dbContext.Categories.Add(category);

            var doctor = new Doctor("Dr Ahmed", "Heart specialist", "user-1", category.Id);
            _dbContext.Doctors.Add(doctor);

            var user = new User { Id = "user-1", UserName = "user1", Email = "user1@test.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var input = new FavouriteCreateDto(doctor.Id);

            _userManagerMock.Setup(u => u.FindByIdAsync(user.Id))
                            .ReturnsAsync(user);
            _repoMock.Setup(t => t.CreateAsync(It.IsAny<Favourite>()))
                .ReturnsAsync(Result<string>.Success("Added to favourites"));

            // Act
            var result = await _favouriteService.AddToFav(input, user.Id);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("Added to favourites successfully");
        }

        [Fact]
        public async Task AddToFavourite_ShouldReturnFail_whenRepositoryFails()
        {
            // Arrange
            var category = new Category("Cardiology");
            _dbContext.Categories.Add(category);

            var doctor = new Doctor("Dr Ahmed", "Heart specialist", "user-1", category.Id);
            _dbContext.Doctors.Add(doctor);

            var user = new User { Id = "user-1", UserName = "user1", Email = "user1@test.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var input = new FavouriteCreateDto(doctor.Id);

            _userManagerMock.Setup(u => u.FindByIdAsync(user.Id))
                            .ReturnsAsync(user);
            _repoMock.Setup(t => t.CreateAsync(It.IsAny<Favourite>()))
                .ReturnsAsync(Result<string>.Fail("DB error"));

            // Act
            var result = await _favouriteService.AddToFav(input, user.Id);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("DB error");
        }

        #endregion

        #region GetUserFavourites

        [Fact]
        public async Task GetUserFavourites_ShouldReturnEmptyList_WhenNoFavourites()
        {
            // Arrange
            var userId = "user-1";
            // Act
            var result = await _favouriteService.GetUserFavourites(userId);
            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserFavourites_ShouldReturnFavourites_WhenFavouritesExist()
        {
            // Arrange
            var category = new Category("Cardiology");
            _dbContext.Categories.Add(category);

            var user = new User { Id = "user-1", UserName = "user1", Email = "user1@test.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) };
            _dbContext.Users.Add(user);

            var doctor = new Doctor("Dr Ahmed", "Heart specialist", user.Id, category.Id);
            _dbContext.Doctors.Add(doctor);

            var favourite = new Favourite(user.Id, doctor.Id);
            _dbContext.Favourites.Add(favourite);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _favouriteService.GetUserFavourites(user.Id);

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(doctor.Id);
        }

        #endregion

        #region RemoveFromFavourites

        [Fact]
        public async Task RemoveFromFavourites_ShouldReturnFail_WhenFavouriteNotFound()
        {
            // Act
            var result = await _favouriteService.RemoveFromFav(Guid.NewGuid(), "");
            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Favourite record not found");
        }

        [Fact]
        public async Task RemoveFromFavourites_ShouldReturnSuccess_WhenFavouriteRemoved()
        {
            // Arrange
            var category = new Category("Cardiology");
            _dbContext.Categories.Add(category);

            var user = new User { Id = "user-1", UserName = "user1", Email = "user1@test.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) };
            _dbContext.Users.Add(user);

            var doctor = new Doctor("Dr Ahmed", "Heart specialist", user.Id, category.Id);
            _dbContext.Doctors.Add(doctor);

            var fav = new Favourite(user.Id, doctor.Id);
            _dbContext.Favourites.Add(fav);
            await _dbContext.SaveChangesAsync();

            _repoMock.Setup(r => r.DeleteAsync(It.IsAny<Favourite>()))
                .ReturnsAsync(Result<string>.Success("Deleted"));

            // Act
            var result = await _favouriteService.RemoveFromFav(doctor.Id, user.Id);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("Removed from favourites successfully");
        }

        [Fact]
        public async Task RemoveFromFavourites_ShouldReturnFail_WhenDeleteFails()
        {
            // Arrange
            var category = new Category("Cardiology");
            _dbContext.Categories.Add(category);

            var user = new User { Id = "user-1", UserName = "user1", Email = "user1@test.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) };
            _dbContext.Users.Add(user);

            var doctor = new Doctor("Dr Ahmed", "Heart specialist", user.Id, category.Id);
            _dbContext.Doctors.Add(doctor);

            var fav = new Favourite(user.Id, doctor.Id);
            _dbContext.Favourites.Add(fav);
            await _dbContext.SaveChangesAsync();

            _repoMock.Setup(r => r.DeleteAsync(It.IsAny<Favourite>()))
                .ReturnsAsync(Result<string>.Fail("Delete failed"));

            // Act
            var result = await _favouriteService.RemoveFromFav(doctor.Id, user.Id);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Failed to remove from favourites");
        }

        #endregion
    }
}
