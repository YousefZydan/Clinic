using Application.AutoMapper;
using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Repository;
using AutoFixture;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using FluentAssertions.Common;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClinicTest.Services
{
    public class AppointmentServiceTests
    {
        ApplicationDbContext _dbContext;
        IMapper _mapper;
        Mock<IGenericRepository<Doctor>> _doctorRepoMock;
        Mock<IGenericRepository<Appointment>> _appointmentRepoMock;
        Mock<INotificationService> _notificationMock;

        AppointmentServices _appointmentService;
        Fixture _fixture;

        public AppointmentServiceTests()
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
            _dbContext = new ApplicationDbContext(options, httpContextAccessorMock.Object);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
            var provider = services.BuildServiceProvider();
            _mapper = provider.GetRequiredService<IMapper>();

            _doctorRepoMock = new Mock<IGenericRepository<Doctor>>();
            _appointmentRepoMock = new Mock<IGenericRepository<Appointment>>();
            _notificationMock = new Mock<INotificationService>();

            _appointmentService = new AppointmentServices(
                _mapper,
                _doctorRepoMock.Object,
                _appointmentRepoMock.Object,
                _dbContext,
                _notificationMock.Object
            );
        }

        #region AddToBooking
        [Fact]
        public async Task AddToBooking_ShouldReturnSuccess_WhenDoctorExistsAndBookingCreated()
        {
            // Arrange
            var category = new Category("Cardiology");
            _dbContext.Categories.Add(category);
            var userId = new User { Id = "user-1", UserName = "user1", Email = "user1@example.com", Name = "User One", Nickname = "u1", DateOfBirth = DateTime.UtcNow.AddYears(-30) };
            _dbContext.Users.Add(userId);
            var doctor = new Doctor("Dr Test", "About", userId.Id, category.Id);
            _dbContext.Doctors.Add(doctor);
            await _dbContext.SaveChangesAsync();

            var dto = new AppoinmentCreate_EditDto
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Hour = TimeOnly.FromDateTime(DateTime.Now)
            };

            _doctorRepoMock
                .Setup(temp => temp.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(doctor);
            _appointmentRepoMock
                .Setup(temp => temp.CreateAsync(It.IsAny<Appointment>()))
                .ReturnsAsync(Result<string>.Success("Created"));
            _notificationMock
                .Setup(temp => temp.AddNotification(It.IsAny<NotificationCreateDto>()))
                .ReturnsAsync(Result<string>.Success("Notified"));

            // Act
            var result = await _appointmentService.AddToBooking(doctor.Id, userId.Id, dto);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("Notified");
        }

        [Fact]
        public async Task AddToBooking_ShouldReturnFail_WhenDoctorDoesNotExist()
        {
            // Arrange
            var dto = new AppoinmentCreate_EditDto
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Hour = TimeOnly.FromDateTime(DateTime.Now)
            };
            _doctorRepoMock
                .Setup(temp => temp.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Doctor)null);
            // Act
            var result = await _appointmentService.AddToBooking(Guid.NewGuid(), "user-1", dto);
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Doctor not found");
        }

        [Fact]
        public async Task AddToBooking_ShouldReturnFail_WhenBookingCreationFails()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var userId = "user-1";

            var doctor = new Doctor("Dr Test", "About", userId, Guid.NewGuid());

            var dto = new AppoinmentCreate_EditDto
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Hour = TimeOnly.FromDateTime(DateTime.Now)
            };

            _doctorRepoMock
                .Setup(temp => temp.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(doctor);
            _appointmentRepoMock
                .Setup(temp => temp.CreateAsync(It.IsAny<Appointment>()))
                .ReturnsAsync(Result<string>.Fail("Create Failed"));

            // Act
            var result = await _appointmentService.AddToBooking(doctorId, userId, dto);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Create Failed");
        }

        [Fact]
        public async Task AddToBooking_ShouldReturnFail_WhenNotificationFails()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var userId = "user-1";

            var doctor = new Doctor("Dr Test", "About", userId, Guid.NewGuid());

            var dto = new AppoinmentCreate_EditDto
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Hour = TimeOnly.FromDateTime(DateTime.Now)
            };

            _doctorRepoMock
                .Setup(temp => temp.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(doctor);
            _appointmentRepoMock
                .Setup(temp => temp.CreateAsync(It.IsAny<Appointment>()))
                .ReturnsAsync(Result<string>.Success("Created"));
            _notificationMock
                .Setup(temp => temp.AddNotification(It.IsAny<NotificationCreateDto>()))
                .ReturnsAsync(Result<string>.Fail("Notification failed"));

            // Act
            var result = await _appointmentService.AddToBooking(doctorId, userId, dto);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Notification failed");
        }
        #endregion

        #region CancelBooking

        [Fact]
        public async Task CancelBooking_ShouldReturnFail_WhenBookingNotFound()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = "user-1";
            // Act
            var result = await _appointmentService.CancelBooking(bookingId, userId);
            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("This booking is not found");
        }

        [Fact]
        public async Task CancelBooking_ShouldReturnFail_WhenUpdateFails()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = "user-1";

            var appointment = new Appointment
            {
                Id = bookingId,
                UserId = userId,
                Doctor = new Doctor("Dr Test", "About", userId, Guid.NewGuid())
            };
            _dbContext.Appointments.Add(appointment);
            await _dbContext.SaveChangesAsync();

            _appointmentRepoMock
                .Setup(temp => temp.UpdateAsync(It.IsAny<Appointment>()))
                .ReturnsAsync(Result<string>.Fail("Update failed"));

            // Act
            var result = await _appointmentService.CancelBooking(bookingId, userId);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Update failed");
        }

        [Fact]
        public async Task CancelBooking_ShouldReturnFail_WhenNotificationFails()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = "user-1";
            var appointment = new Appointment
            {
                Id = bookingId,
                UserId = userId,
                Doctor = new Doctor("Dr Test", "About", userId, Guid.NewGuid())
            };
            _dbContext.Appointments.Add(appointment);
            await _dbContext.SaveChangesAsync();

            _appointmentRepoMock
                .Setup(temp => temp.UpdateAsync(It.IsAny<Appointment>()))
                .ReturnsAsync(Result<string>.Success("Updated"));

            _notificationMock
                .Setup(temp => temp.AddNotification(It.IsAny<NotificationCreateDto>()))
                .ReturnsAsync(Result<string>.Fail("Failed to add notification"));

            // Act
            var result = await _appointmentService.CancelBooking(bookingId, userId);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Failed to add notification");
        }

        [Fact]
        public async Task CancelBooking_ShouldReturnSuccess_WhenEveryThingIsValid()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = "user-1";
            var appointment = new Appointment
            {
                Id = bookingId,
                UserId = userId,
                Doctor = new Doctor("Dr Test", "About", userId, Guid.NewGuid())
            };
            _dbContext.Appointments.Add(appointment);
            await _dbContext.SaveChangesAsync();

            _appointmentRepoMock
                .Setup(temp => temp.UpdateAsync(It.IsAny<Appointment>()))
                .ReturnsAsync(Result<string>.Success("Updated"));

            _notificationMock
                .Setup(temp => temp.AddNotification(It.IsAny<NotificationCreateDto>()))
                .ReturnsAsync(Result<string>.Success("Notification added and sent successfully"));

            // Act
            var result = await _appointmentService.CancelBooking(bookingId, userId);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("Notification added and sent successfully");
        }

        #endregion

        #region EditBooking

        [Fact]
        public async Task EditBooking_ShouldReturnFail_WhenBookingNotFound()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = "user-1";

            var dto = new AppoinmentCreate_EditDto
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Hour = TimeOnly.FromDateTime(DateTime.Now)
            };

            _appointmentRepoMock
                .Setup(temp => temp.GetByAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment>());

            // Act
            var result = await _appointmentService.EditBooking(bookingId, userId, dto);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("This booking is not found");
        }

        [Fact]
        public async Task EditBooking_ShouldReturnFail_WhenUpdateFails()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = "user-1";

            var appointment = new Appointment
            {
                Id = bookingId,
                UserId = userId,
                Date = DateOnly.FromDateTime(DateTime.Now),
                Hour = new TimeOnly(10, 0)
            };
            var dto = new AppoinmentCreate_EditDto
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                Hour = new TimeOnly(12, 0)
            };

            _appointmentRepoMock
                .Setup(temp => temp.GetByAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment> { appointment });

            _appointmentRepoMock
                .Setup(temp => temp.UpdateAsync(It.IsAny<Appointment>()))
                .ReturnsAsync(Result<string>.Fail("No changes were saved"));

            // Act
            var result = await _appointmentService.EditBooking(bookingId, userId, dto);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("No changes were saved");
        }

        [Fact]
        public async Task EditBooking_ShouldReturnFail_WhenNotificationFails()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = "user-1";
            var appointment = new Appointment
            {
                Id = bookingId,
                UserId = userId,
                Date = DateOnly.FromDateTime(DateTime.Now),
                Hour = new TimeOnly(10, 0)
            };
            var dto = new AppoinmentCreate_EditDto
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                Hour = new TimeOnly(12, 0)
            };

            _appointmentRepoMock
                .Setup(x => x.GetByAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment> { appointment });

            _appointmentRepoMock
                .Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
                .ReturnsAsync(Result<string>.Success("Updated Successfully"));

            _notificationMock
                .Setup(x => x.AddNotification(It.IsAny<NotificationCreateDto>()))
                .ReturnsAsync(Result<string>.Fail("Failed to add notification"));

            // Act
            var result = await _appointmentService.EditBooking(bookingId, userId, dto);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Failed to add notification");
        }

        [Fact]
        public async Task EditBooking_ShouldReturnSuccess_WhenEverythingIsValid()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = "user-1";
            var appointment = new Appointment
            {
                Id = bookingId,
                UserId = userId,
                Date = DateOnly.FromDateTime(DateTime.Now),
                Hour = new TimeOnly(10, 0)
            };
            var dto = new AppoinmentCreate_EditDto
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                Hour = new TimeOnly(14, 0)
            };

            _appointmentRepoMock
                .Setup(x => x.GetByAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment> { appointment });

            _appointmentRepoMock
                .Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
                .ReturnsAsync(Result<string>.Success("Updated Successfully"));

            _notificationMock
                .Setup(x => x.AddNotification(It.IsAny<NotificationCreateDto>()))
                .ReturnsAsync(Result<string>.Success("Notification added and sent successfully"));

            // Act
            var result = await _appointmentService.EditBooking(bookingId, userId, dto);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("Notification added and sent successfully");
        }

        #endregion

        #region GetUserBookings

        [Fact]
        public async Task GetUserBookings_ShouldReturnFail_WhenStatusIsEmpty()
        {
            // Arrange

            // Act
            var result = await _appointmentService.GetUserBookings("user-1", "");
            
            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Status is required");
        }

        [Fact]
        public async Task GetUserBookings_ShouldReturnFail_WhenStatusIsInvalid()
        {
            // Arrange

            // Act
            var result = await _appointmentService.GetUserBookings("user-1", "InvalidStatus");
            
            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Invalid status value");
        }

        [Fact]
        public async Task GetUserBookings_ShouldReturnEmptyList_WhenNoData()
        {
            // Arrange

            // Act
            var result = await _appointmentService.GetUserBookings("user-1", "UpComming");

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserBookings_ShouldReturnListOfBookings_WhenDataExists()
        {
            // Arrange
            var userId = "user-1";
            var category = new Category("General");
            _dbContext.Categories.Add(category); 
            var user = new User { Id = userId, UserName = "test", Email = "test@test.com", Name = "Test", Nickname = "tester", DateOfBirth = DateTime.UtcNow.AddYears(-30) };
            _dbContext.Users.Add(user); 
            var doctor = new Doctor("Dr Test", "About", userId, category.Id);
            _dbContext.Doctors.Add(doctor); 
            await _dbContext.SaveChangesAsync();

            var appointments = new List<Appointment>
            {
                new Appointment { Id = Guid.NewGuid(), UserId = userId, DoctorId = doctor.Id, Date = DateOnly.FromDateTime(DateTime.Now), Hour = new TimeOnly(10, 0), Status = AppointmentStatus.UpComming },
                new Appointment { Id = Guid.NewGuid(), UserId = userId, DoctorId = doctor.Id, Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), Hour = new TimeOnly(11, 0), Status = AppointmentStatus.UpComming }
            };
            _dbContext.Appointments.AddRange(appointments);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _appointmentService.GetUserBookings(userId, "UpComming");

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.Select(x => x.Hour)
                       .Should()
                       .Contain(new TimeOnly(10, 0))
                       .And.Contain(new TimeOnly(11, 0)); 
            result.Data.All(x => x.DoctorName == "Dr Test").Should().BeTrue(); 
        }

        #endregion

        #region NonAvailableAppointments

        [Fact]
        public async Task NonAvailableAppointments_ShouldReturnFail_WhenDoctorNotFound()
        {
            // Arrange
            var doctorId = Guid.NewGuid();

            // Act
            var result = await _appointmentService.NonAvailableAppointments(doctorId);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task NonAvailableAppointments_ShouldReturnOnlyUpComingAppointments()
        {
            // Arrange
            var doctorId = Guid.NewGuid();

            var appointments = new List<Appointment>()
            {
                new Appointment { DoctorId = doctorId, Date = DateOnly.FromDateTime(DateTime.Today), Hour = new TimeOnly(9, 0), Status = AppointmentStatus.Cancelled },
                new Appointment { DoctorId = doctorId, Date = DateOnly.FromDateTime(DateTime.Today), Hour = new TimeOnly(10, 0), Status = AppointmentStatus.UpComming }
            };
            _appointmentRepoMock
                .Setup(x => x.GetByAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync((Expression<Func<Appointment, bool>> predicate) =>
                appointments.Where(predicate.Compile()).ToList()
                );

            // Act
            var result = await _appointmentService.NonAvailableAppointments(doctorId);

            // Assert
            //result.Succeeded.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().Hour.Should().Be(new TimeOnly(10, 0));
        }

        [Fact]
        public async Task NonAvailableAppointments_ShouldReturnAppointments_WhenExist()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var appointments = new List<Appointment>()
            {
                new Appointment { DoctorId = doctorId, Date = DateOnly.FromDateTime(DateTime.Today), Hour = new TimeOnly(9, 0), Status = AppointmentStatus.UpComming },
                new Appointment { DoctorId = doctorId, Date = DateOnly.FromDateTime(DateTime.Today), Hour = new TimeOnly(10, 0), Status = AppointmentStatus.UpComming }
            };
            _appointmentRepoMock
                .Setup(x => x.GetByAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync((Expression<Func<Appointment, bool>> predicate) =>
                appointments.Where(predicate.Compile()).ToList()
                );

            // Act
            var result = await _appointmentService.NonAvailableAppointments(doctorId);
            
            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        #endregion

    }
}
