using CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAS.FinancePortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
	public class UserAccessTypeConfig : IEntityTypeConfiguration<UserAccessType>
	{
		public void Configure(EntityTypeBuilder<UserAccessType> builder)
		{
			builder.HasKey(e => e.Id).HasName("PK_user_access");

			builder.ToTable("user_access_types");

			builder.Property(e => e.Id)
				.ValueGeneratedNever()
				.HasColumnName("id");
			builder.Property(e => e.AccessType)
				.IsRequired()
				.HasMaxLength(50)
				.HasColumnName("access_type");
			builder.Property(e => e.IsActive)
				.HasDefaultValue(true)
				.HasColumnName("is_active");
		}
	}
}
