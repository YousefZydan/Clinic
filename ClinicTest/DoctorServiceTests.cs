using Application.AutoMapper;
using Application.Dtos;
using AutoFixture;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
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
    public class DoctorServiceTests
    {
        ApplicationDbContext _dbContext;
        IMapper _mapper;
        DoctorService _doctorService;
        Fixture _fixture;

        public DoctorServiceTests()
        {
            _fixture = new Fixture();

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

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

            _doctorService = new DoctorService(_dbContext, _mapper);
        }

        #region GetAllDoctors
        [Fact]
        public async Task GetAllDoctors_ShouldReturnDoctors_WhenDoctorsExist()
        {
            // Arrange
            var category = new Category("Cardiology");
            _dbContext.Categories.Add(category);

            List<User> users = new List<User>
            {
                new User {Id = "user-1", UserName = "user1", Email = "user1@example.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) },
                new User {Id = "user-2", UserName = "user2", Email = "user2@example.com", Name = "User two", Nickname = "u2", DateOfBirth = DateTime.UtcNow.AddYears(-32) },
                new User {Id = "user-3", UserName = "user3", Email = "user3@example.com", Name = "User three", Nickname = "u4", DateOfBirth = DateTime.UtcNow.AddYears(-28) }
            };

            _dbContext.Users.AddRange(users);
            await _dbContext.SaveChangesAsync();

            var doctors = new List<Doctor>
            {
                new Doctor("Dr. Ahmed", "Heart specialist", users[0].Id, category.Id),
                new Doctor("Dr. Sara", "Heart surgeon", users[1].Id, category.Id),
                new Doctor("Dr. Ali", "Cardiology consultant", users[2].Id, category.Id)
            };

            await _dbContext.Doctors.AddRangeAsync(doctors);
            await _dbContext.SaveChangesAsync();

            // Act
            List<DoctorDto> doctorsList = await _doctorService.GetAllDoctors();

            // Assert
            doctorsList.Should().NotBeNull();
            doctorsList.Select(d => d.Name)
                .Should()
                .BeEquivalentTo(doctors.Select(d => d.Name));
        }

        [Fact]
        public async Task GetAllDoctors_ShouldReturnEmptyList_WhenNoDoctorsExist()
        {
            // Act
            var doctorsList = await _doctorService.GetAllDoctors();

            //Assert            
            doctorsList.Should().BeEmpty();
        }
        #endregion

        #region GetDoctorByCategoryID
        [Fact]
        public async Task GetDoctorByCategoryId_ShouldReturnDoctors_WhenCategoryExists()
        {
            // Arrange
            var categories = new List<Category>()
            {
                new Category("Cardiology"),
                new Category("Neurology")
            };
            _dbContext.Categories.AddRange(categories);

            List<User> users = new List<User>
            {
                new User {Id = "user-1", UserName = "user1", Email = "user1@example.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) },
                new User {Id = "user-2", UserName = "user2", Email = "user2@example.com", Name = "User two", Nickname = "u2", DateOfBirth = DateTime.UtcNow.AddYears(-32) },
                new User {Id = "user-3", UserName = "user3", Email = "user3@example.com", Name = "User three", Nickname = "u4", DateOfBirth = DateTime.UtcNow.AddYears(-28) }
            };
            _dbContext.Users.AddRange(users);
            await _dbContext.SaveChangesAsync();

            var doctors = new List<Doctor>
            {
                new Doctor("Dr. Ahmed", "Heart specialist", users[0].Id, categories[0].Id),
                new Doctor("Dr. Sara", "Heart surgeon", users[1].Id, categories[1].Id),
                new Doctor("Dr. Ali", "Cardiology consultant", users[2].Id, categories[0].Id)
            };
            await _dbContext.Doctors.AddRangeAsync(doctors);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _doctorService.GetDoctorByCategoryId(categories[0].Id);

            // Assert
            result.Should().HaveCount(2);
            result.First().CategoryName.Should().Be(categories[0].Name);
        }

        [Fact]
        public async Task GetDoctorByCategoryId_ShouldReturnEmptyList_WhenCategoryDoesNotExist()
        {
            // Act
            var result = await _doctorService.GetDoctorByCategoryId(Guid.NewGuid());
            // Assert
            result.Should().BeEmpty();
        }
        #endregion

        #region GetDoctorByCategoryName

        [Fact]
        public async Task GetDoctorByCategoryName_ShouldReturnDoctors_WhenCategoryExists()
        {
            // Arrange
            var categories = new List<Category>()
            {
                new Category("Cardiology"),
                new Category("Neurology")
            };
            _dbContext.Categories.AddRange(categories);

            List<User> users = new List<User>
            {
                new User {Id = "user-1", UserName = "user1", Email = "user1@example.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) },
                new User {Id = "user-2", UserName = "user2", Email = "user2@example.com", Name = "User two", Nickname = "u2", DateOfBirth = DateTime.UtcNow.AddYears(-32) },
                new User {Id = "user-3", UserName = "user3", Email = "user3@example.com", Name = "User three", Nickname = "u4", DateOfBirth = DateTime.UtcNow.AddYears(-28) }
            };
            _dbContext.Users.AddRange(users);
            await _dbContext.SaveChangesAsync();

            var doctors = new List<Doctor>
            {
                new Doctor("Dr. Ahmed", "Heart specialist", users[0].Id, categories[1].Id),
                new Doctor("Dr. Sara", "Heart surgeon", users[1].Id, categories[1].Id),
                new Doctor("Dr. Ali", "Cardiology consultant", users[2].Id, categories[0].Id)
            };
            await _dbContext.Doctors.AddRangeAsync(doctors);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _doctorService.GetDoctorByCategoryName(categories[0].Name);

            // Assert
            result.Should().HaveCount(1);
            result.First().CategoryName.Should().Be(categories[0].Name);
        }

        [Fact]
        public async Task GetDoctorByCategoryName_ShouldReturnEmptyList_WhenCategoryDoesNotExist()
        {
            // Arrange
            var category = null as string;
            // Act
            var result = await _doctorService.GetDoctorByCategoryName(category);
            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetDoctorByName

        [Fact]
        public async Task GetDoctorByName_ShouldReturnDoctors_WhenDoctorsExist()
        {
            // Arrange
            var category = new Category("Cardiology");
            _dbContext.Categories.Add(category);

            var user = new User { Id = "user-1", UserName = "user1", Email = "user1@example.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var doctor = new Doctor("Dr. Ahmed", "Heart specialist", user.Id, category.Id);
            _dbContext.Doctors.Add(doctor);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _doctorService.GetDoctorByName("Dr. Ahmed");

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Dr. Ahmed");
        }

        [Fact]
        public async Task GetDoctorByName_ShouldReturnEmptyList_WhenNoDoctorsExist()
        {
            // Act
            var result = await _doctorService.GetDoctorByName("");
            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}
