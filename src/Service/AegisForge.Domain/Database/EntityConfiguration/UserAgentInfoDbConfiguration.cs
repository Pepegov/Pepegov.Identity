using AegisForge.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AegisForge.Domain.Database.EntityConfiguration;

public class UserAgentInfoDbConfiguration : IEntityTypeConfiguration<UserAgentInfo>
{
    public void Configure(EntityTypeBuilder<UserAgentInfo> builder)
    {
        builder.Property<Guid>("UserConnectionInfoId");       
    }
}