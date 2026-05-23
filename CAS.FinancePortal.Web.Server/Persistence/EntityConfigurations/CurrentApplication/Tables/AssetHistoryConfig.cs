
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Agile.OpsPortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
    public class AssetHistoryConfig : IEntityTypeConfiguration<AssetHistory>
    {
        public void Configure(EntityTypeBuilder<AssetHistory> builder)
        {
            builder.ToTable("AssetHistory");

            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.Property(e => e.CreatedDate).HasColumnType("datetime");

            builder.HasOne(d => d.AssetHistoryStatus).WithMany(p => p.AssetHistories)
                .HasForeignKey(d => d.AssetHistoryStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssetHistory_AssetHistoryStatus");

            // builder.HasOne(d => d.Asset).WithMany(p => p.AssetHistories)
            //     .HasForeignKey(d => d.AssetId)
            //     .OnDelete(DeleteBehavior.ClientSetNull)
            //     .HasConstraintName("FK_AssetHistory_Assets");
        }
    }
}
