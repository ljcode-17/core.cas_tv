using CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAS.FinancePortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
	public class UserAccessConfig : IEntityTypeConfiguration<UserAccess>
	{
		public void Configure(EntityTypeBuilder<UserAccess> builder)
		{
			builder.HasKey(e => e.Id).HasName("PK_user_access_1");
			builder.HasAlternateKey(e => e.UserId).HasName("AK_user_access_user_id");

			builder.ToTable("user_access");

			builder.Property(e => e.Id)
				.ValueGeneratedNever()
				.HasColumnName("id");
			builder.Property(e => e.CreatedBy)
				.IsRequired()
				.HasColumnName("created_by");
			builder.Property(e => e.CreatedDate)
				.IsRequired()
				.HasColumnName("created_date");
			builder.Property(e => e.IsActive)
				.HasDefaultValue(true)
				.HasColumnName("is_active");
			builder.Property(e => e.UserAccessId).HasColumnName("user_access_id");
			builder.Property(e => e.UserId).HasColumnName("user_id");

			builder.HasOne(d => d.UserAccessNavigation).WithMany(p => p.UserAccesses)
				.HasForeignKey(d => d.UserAccessId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_user_access_user_access_types");
		}
	}
}
