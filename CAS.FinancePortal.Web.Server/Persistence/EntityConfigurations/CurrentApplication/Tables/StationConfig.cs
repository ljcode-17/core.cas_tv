using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Agile.OpsPortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
    public class StationConfig : IEntityTypeConfiguration<Station>
    {
        public void Configure(EntityTypeBuilder<Station> builder)
        {
            builder.Property(e => e.CreatedDate).HasColumnType("datetime");
            builder.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasAnnotation("Relational:DefaultConstraintName", "DF_Stations_IsActive");
            builder.Property(e => e.Station1)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("Station");
            builder.HasOne(d => d.Cluster).WithMany(p => p.Stations)
                .HasForeignKey(d => d.ClusterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stations_Clusters");
        }
    }
}
