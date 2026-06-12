using Application.Dtos;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class AppointmentReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AppointmentReminderService(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var notificationService = scope.ServiceProvider
                .GetRequiredService<INotificationService>();

            var appointments = await db.Appointments
                .Where(x => x.Status ==
                            AppointmentStatus.UpComming)
                .ToListAsync();

            var now = DateTime.Now;
            DateTime appointmentDateTime;

            foreach (var appointment in appointments)
            {
                 appointmentDateTime =
                    appointment.Date.ToDateTime(appointment.Hour);

                var remaining =
                    appointmentDateTime - now;

                // قبل بساعة
                if (!appointment.OneHourReminderSent &&
                    remaining <= TimeSpan.FromHours(1) &&
                    remaining > TimeSpan.FromMinutes(59))
                {
                    await notificationService.AddNotification(
                        new NotificationCreateDto
                        {
                            UserId = appointment.UserId,
                            Title = "Appointment Reminder",
                            Message =
                                "Reminder: Your appointment is after 1 hour"
                        });

                    appointment.OneHourReminderSent = true;
                }

                // قبل بنص ساعة
                if (!appointment.ThirtyMinutesReminderSent &&
                    remaining <= TimeSpan.FromMinutes(30) &&
                    remaining > TimeSpan.FromMinutes(29))
                {
                    await notificationService.AddNotification(
                        new NotificationCreateDto
                        {
                            UserId = appointment.UserId,
                            Title = "Appointment Reminder",
                            Message =
                                "Reminder: Your appointment is after 30 minutes"
                        });

                    appointment.ThirtyMinutesReminderSent = true;
                }
            }


            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken);
        }
    }
}
