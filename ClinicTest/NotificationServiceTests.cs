using Application.AutoMapper;
using Application.Dtos;
using Application.Helpers;
using Application.Interfaces.Firebase;
using Application.Repository;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
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
    public class NotificationServiceTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
        private readonly Mock<IGenericRepository<Notification>> _notificationRepoMock;
        private readonly Mock<IFirebaseNotificationService> _firebaseServiceMock;

        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
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

            _hubContextMock = new Mock<IHubContext<NotificationHub>>();

            _notificationRepoMock =
                new Mock<IGenericRepository<Notification>>();

            _firebaseServiceMock =new Mock<IFirebaseNotificationService>();

            _notificationService = new NotificationService(
                _hubContextMock.Object,
                _mapper,
                _notificationRepoMock.Object,
                _dbContext,
                _firebaseServiceMock.Object);
        }

        #region AddNotification

        [Fact]
        public async Task AddNotification_ShouldReturnSuccess_WhenNotificationCreated()
        {
            //Arrange
            var dto = new NotificationCreateDto { UserId = "user-1", Title = "Test Title", Message = "Test Message" };

            _notificationRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
                .ReturnsAsync(Result<string>.Success("Created"));

            var clientProxyMock = new Mock<IClientProxy>();
            var hubClientsMock = new Mock<IHubClients>();

            hubClientsMock
                .Setup(x => x.User(dto.UserId))
                .Returns(clientProxyMock.Object);
            _hubContextMock
                .Setup(x => x.Clients)
                .Returns(hubClientsMock.Object);

            //Act
            var result = await _notificationService.AddNotification(dto);

            //Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("Notification added and sent successfully");
        }

        [Fact]
        public async Task AddNotification_ShouldReturnFail_WhenCreateFails()
        {
            //Arrange
            var dto = new NotificationCreateDto { UserId = "user-1", Title = "Test Title", Message = "Test Message" };

            _notificationRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
                .ReturnsAsync(Result<String>.Fail("Creation failed"));

            //Act
            var result = await _notificationService.AddNotification(dto);

            //Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Creation failed");
        }

        #endregion

        #region GetNotifications

        [Fact]
        public async Task GetNotifications_ShouldReturnNotifications_WhenNotificationsExist()
        {
            // Arrange
            var userId = "user-1";

            var notifications = new List<Notification>
            {
                new Notification(userId, "Title1", "Message1"),
                new Notification(userId, "Title2", "Message2")
            };

            _dbContext.Notifications.AddRange(notifications);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _notificationService.GetNotifications(userId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().HaveCount(2);

            result.Data!.Select(x => x.Title)
                .Should().Contain(new[] { "Title1", "Title2" });
        }

        [Fact]
        public async Task GetNotifications_ShouldReturnEmptyList_WhenNoNotificationsExist()
        {
            // Arrange
            var userId = "user-1";

            // Act
            var result = await _notificationService.GetNotifications(userId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region MarkNotificationAsRead

        [Fact]
        public async Task MarkNotificationAsRead_ShouldReturnFail_WhenNotificationNotFound()
        {
            //Arrange
            var notificationId = Guid.NewGuid();

            //Act
            var result = await _notificationService.MarkNotificationAsRead(notificationId);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Notification not found.");
        }

        [Fact]
        public async Task MarkNotificationAsRead_ShouldReturnFail_WhenUpdateFails()
        {
            //Arrange
            var notification = new Notification("user-1", "title", "message");

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            _notificationRepoMock
                .Setup(x => x.UpdateAsync(It.IsAny<Notification>()))
                .ReturnsAsync(Result<string>.Fail("Update failed"));

            //Act
            var result = await _notificationService.MarkNotificationAsRead(notification.Id);

            //Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Update failed");
        }

        [Fact]
        public async Task MarkNotificationAsRead_ShouldReturnSuccess_WhenNotificationExists()
        {
            // Arrange
            var notification = new Notification("user-1", "title", "message");

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            _notificationRepoMock
                .Setup(x => x.UpdateAsync(It.IsAny<Notification>()))
                .ReturnsAsync(Result<string>.Success("Updated"));

            // Act
            var result = await _notificationService.MarkNotificationAsRead(notification.Id);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be("Notification marked as read successfully.");
            notification.IsRead.Should().BeTrue();
        }

        #endregion

    }
}
