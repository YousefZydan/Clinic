using Domain.Entities;
using Domain.Premitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;
using System.Security.Claims;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor
        ) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ResetCode> ResetCodes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DoctorDetails> DoctorDetails { get; set; }
        public DbSet<WorkingTime> WorkingTimes { get; set; }
        //public DbSet<Appointment> Appointments { get; set; }

        


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }




        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return await base.SaveChangesAsync(cancellationToken);
        }


        private void ApplyAuditInfo()
        {
            var username = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ?? "System";

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Audited<Guid> &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (Audited<Guid>)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedBy = username;
                    entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.ModifiedBy = username;
                    entity.ModifiedAt = DateTime.UtcNow;
                }
            }
        }
    }
}

