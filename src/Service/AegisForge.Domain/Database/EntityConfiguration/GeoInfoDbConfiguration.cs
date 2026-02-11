using AegisForge.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AegisForge.Domain.Database.EntityConfiguration;

public class GeoInfoDbConfiguration : IEntityTypeConfiguration<GeoInfo>
{
    public void Configure(EntityTypeBuilder<GeoInfo> builder)
    {
        builder.Property<Guid>("UserConnectionInfoId");
    }
}