
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Agile.OpsPortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
    public class AssetHistoryStatusConfig : IEntityTypeConfiguration<AssetHistoryStatus>
    {
        public void Configure(EntityTypeBuilder<AssetHistoryStatus> builder)
        {
            builder.ToTable("AssetHistoryStatus");

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.CreatedDate).HasColumnType("datetime");
            builder.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasAnnotation("Relational:DefaultConstraintName", "DF_AssetHistoryStatus_IsActive");
            builder.Property(e => e.StatusName)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
