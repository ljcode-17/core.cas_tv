
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Agile.OpsPortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
    public class AssetEmployeeHistoryConfig : IEntityTypeConfiguration<AssetEmployeeHistory>
    {
        public void Configure(EntityTypeBuilder<AssetEmployeeHistory> builder)
        {
            builder.ToTable("AssetEmployeeHistory");

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.CreatedDate).HasColumnType("datetime");
            builder.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasAnnotation("Relational:DefaultConstraintName", "DF_AssetEmployeeHistory_IsActive");

            builder.HasOne(d => d.AssetHistory).WithMany(p => p.AssetEmployeeHistories)
                .HasForeignKey(d => d.AssetHistoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssetEmployeeHistory_AssetHistory");
        }
    }
}
