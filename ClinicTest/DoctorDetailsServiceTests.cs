using Application.AutoMapper;
using AutoFixture;
using AutoMapper;
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
using Domain.Entities;
using FluentAssertions;

namespace ClinicTest.Services
{
    public class DoctorDetailsServiceTests
    {
        ApplicationDbContext _dbContext;
        IMapper _mapper;
        DoctorDetailsService _doctorDetailsService;

        public DoctorDetailsServiceTests()
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

            _doctorDetailsService = new DoctorDetailsService(_dbContext, _mapper);

        }

        #region GetByDoctorID
        [Fact]
        public async Task GetByDoctorId_ShouldReturnDoctorDetails_WhenDoctorExists()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "testuser@example.com", Name = "Test User", Nickname = "tester", DateOfBirth = DateTime.UtcNow.AddYears(-30) };
            _dbContext.Users.Add(user);

            var category = new Category("General");
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            var doctor = new Doctor("Dr Name", "About Dr", user.Id, category.Id);
            _dbContext.Doctors.Add(doctor);
            await _dbContext.SaveChangesAsync();

            var doctorDetails = new List<DoctorDetails>();
            for (int i = 0; i < 3; i++)
            {
                doctorDetails.Add(new DoctorDetails
                {
                    AboutMe = $"About {i}",
                    PatientsCount = 100 + i,
                    YearsOfExperience = 5 + i,
                    Rating = 4.5 + i * 0.1,
                    ReviewsCount = 10 + i,
                    DoctorId = doctor.Id
                });
            }

            _dbContext.DoctorDetails.AddRange(doctorDetails);
            await _dbContext.SaveChangesAsync();

            //Act
            var result = await _doctorDetailsService.GetByDoctorId(doctor.Id);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.All(d => d.DoctorName != null).Should().BeTrue();

        }

        [Fact]
        public async Task GetByDoctorId_ShouldReturnEmptyList_WhenDoctorNotFound()
        {
            //Arrange
            var doctorId = Guid.NewGuid();

            //Act 
            var result = await _doctorDetailsService.GetByDoctorId(doctorId);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
        #endregion

        #region Rating

        [Fact]
        public async Task Rating_ShouldReturnFail_WhenDoctorDetailsNotFound()
        {
            // Arrange
            var doctorId = Guid.NewGuid();

            // Act
            var result = await _doctorDetailsService.Rating(doctorId, 4.5);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Doctor details not found");
        }

        [Fact]
        public async Task Rating_ShouldReturnSuccess_WhenRatingAdded()
        {
            // Arrange
            var doctorId = Guid.NewGuid();

            var doctor = new Doctor(
                "Dr",
                "About",
                "user",
                Guid.NewGuid());

            typeof(Doctor)
                .GetProperty(nameof(Doctor.Id))!
                .SetValue(doctor, doctorId);

            var doctorDetails = new DoctorDetails
            {
                DoctorId = doctorId,
                Rating = 4,
                ReviewsCount = 1,
                PatientsCount = 10,
                YearsOfExperience = 5,
                AboutMe = "test"
            };

            _dbContext.Doctors.Add(doctor);
            _dbContext.DoctorDetails.Add(doctorDetails);
            await _dbContext.SaveChangesAsync();

            var rate = 5.0;

            // Act
            var result = await _doctorDetailsService.Rating(doctorId, rate);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("Rating added successfully");

            var updated = _dbContext.DoctorDetails.First();

            updated.ReviewsCount.Should().Be(2);
            updated.Rating.Should().BeApproximately(4.5, 0.01);
        }

        #endregion
    }
}
