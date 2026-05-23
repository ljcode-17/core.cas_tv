using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Core.Agile.OpsPortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
    public class SeatAssignmentConfig : IEntityTypeConfiguration<SeatAssignment>
    {
        public void Configure(EntityTypeBuilder<SeatAssignment> builder)
        {
            builder.Property(e => e.AssignedDate).HasColumnType("datetime");
            builder.Property(e => e.CreatedDate).HasColumnType("datetime");
            builder.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasAnnotation("Relational:DefaultConstraintName", "DF_SeatAssignments_IsActive");
            builder.Property(e => e.VacatedDate).HasColumnType("datetime");
            builder.HasOne(d => d.Station).WithMany(p => p.SeatAssignments)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeatAssignments_Stations");
        }
    }
}
