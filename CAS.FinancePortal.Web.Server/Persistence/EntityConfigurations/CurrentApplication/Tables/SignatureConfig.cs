using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAS.FinancePortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
    public class SignatureConfig : IEntityTypeConfiguration<Signature>
    {
        public void Configure(EntityTypeBuilder<Signature> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.Property(e => e.CreatedDate).HasColumnType("datetime");
            builder.Property(e => e.IsActive).HasDefaultValue(true);
            builder.Property(e => e.Esignature)
                .IsRequired()
                .HasColumnName("ESignature");

            builder.HasOne<VwEmployee>()
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Signatures_Vw_Employees");
        }
    }
}
