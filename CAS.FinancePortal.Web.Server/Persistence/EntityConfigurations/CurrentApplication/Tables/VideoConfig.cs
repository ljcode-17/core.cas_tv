using CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAS.FinancePortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
    public class VideoConfig : IEntityTypeConfiguration<Video>
    {
        public void Configure(EntityTypeBuilder<Video> builder)
        {
            builder.ToTable("Videos");

            builder.HasKey(v => v.Id);
            builder.Property(v => v.FileName).HasMaxLength(260).IsRequired();
            builder.Property(v => v.FilePath).HasMaxLength(500).IsRequired();
            builder.Property(v => v.ContentType).HasMaxLength(100);
            builder.Property(v => v.FileSize);
            builder.Property(v => v.Status).HasMaxLength(16).HasDefaultValue("Active").IsRequired();
            builder.Property(v => v.UploadedBy);
            builder.Property(v => v.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.Property(v => v.UpdatedAt);
            builder.Property(v => v.IsDeleted).HasDefaultValue(false);

            builder.HasOne(v => v.UploadedByUserAccess)
                .WithMany(u => u.Videos)
                .HasForeignKey(v => v.UploadedBy)
                .HasPrincipalKey(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Videos_user_access_user_id");

            builder.HasIndex(v => v.Status);
            builder.HasIndex(v => v.CreatedAt);
        }
    }
}
