using AegisForge.Domain.Aggregate;
using AegisForge.Domain.Entity;
using AegisForge.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AegisForge.Domain.Database.EntityConfiguration;

public class ApplicationSessionDbConfiguration : IEntityTypeConfiguration<ApplicationSession>
{
    public void Configure(EntityTypeBuilder<ApplicationSession> builder)
    {
        builder.HasIndex(x => x.SessionState).IsUnique();
        
        builder
            .Property(x => x.SessionStatusType)
            .HasConversion<EnumToStringConverter<SessionStatusType>>();

        builder.HasOne(x => x.UserConnectionInfo)
            .WithOne()
            .HasForeignKey<UserConnectionInfo>("SessionId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}