using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Core.Agile.OpsPortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
    public class AssetDeploymentConfig : IEntityTypeConfiguration<AssetDeployment>
    {
        public void Configure(EntityTypeBuilder<AssetDeployment> builder)
        {
            builder.ToTable("AssetDeployment");

            builder.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasAnnotation("Relational:DefaultConstraintName", "DF_AssetDeployment_IsActive");

            // builder.HasOne(d => d.Asset).WithMany(p => p.AssetDeployments)
            //     .HasForeignKey(d => d.AssetId)
            //     .OnDelete(DeleteBehavior.ClientSetNull)
            //     .HasConstraintName("FK_AssetDeployment_Assets");
            builder.HasOne(d => d.Station).WithMany(p => p.AssetDeployments)
                .HasForeignKey(d => d.StationId)
                .HasConstraintName("FK_AssetDeployment_Stations");
        }
    }
}
