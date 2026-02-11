using AegisForge.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AegisForge.Domain.Database.EntityConfiguration;

public class UserConnectionInfoDbConfiguration : IEntityTypeConfiguration<UserConnectionInfo>
{
    public void Configure(EntityTypeBuilder<UserConnectionInfo> builder)
    {
        builder.Property<Guid>("SessionId");
        
        builder
            .HasOne(o => o.GeoData)
            .WithOne()
            .HasForeignKey<GeoInfo>("UserConnectionInfoId")
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(o => o.UserAgent)
            .WithOne()
            .HasForeignKey<UserAgentInfo>("UserConnectionInfoId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}