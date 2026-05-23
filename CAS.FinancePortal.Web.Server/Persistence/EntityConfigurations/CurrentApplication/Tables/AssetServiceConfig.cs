
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Agile.OpsPortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
	public class AssetServiceConfig : IEntityTypeConfiguration<AssetService>
	{
		public void Configure(EntityTypeBuilder<AssetService> builder)
		{
            builder.Property(e => e.Description).IsRequired();
            builder.Property(e => e.LaborCost).HasColumnType("decimal(18, 2)");
            builder.Property(e => e.PartsCost).HasColumnType("decimal(18, 2)");
            builder.Property(e => e.ServicedBy)
                .IsRequired()
                .HasMaxLength(100);
            // builder.HasOne(d => d.Asset).WithMany(p => p.AssetServices)
            //     .HasForeignKey(d => d.AssetId)
            //     .OnDelete(DeleteBehavior.ClientSetNull)
            //     .HasConstraintName("FK_AssetServices_Assets");
            builder.HasOne(d => d.AssetServiceType).WithMany(p => p.AssetServices)
                .HasForeignKey(d => d.AssetServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssetServices_AssetServicesTypes");
        }
	}
}
