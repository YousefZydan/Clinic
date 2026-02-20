using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class DoctorConfig : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.HasOne(x => x.User)
                .WithMany(x => x.Doctors)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();


            builder.HasOne(x => x.Category)
                .WithMany(x => x.Doctors)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();



        }
    }
}

