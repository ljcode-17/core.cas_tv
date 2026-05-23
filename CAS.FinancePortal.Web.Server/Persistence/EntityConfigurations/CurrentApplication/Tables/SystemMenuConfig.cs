using CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAS.FinancePortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables
{
	public class SystemMenuConfig : IEntityTypeConfiguration<SystemMenu>
	{
		public void Configure(EntityTypeBuilder<SystemMenu> builder)
		{
			builder.ToTable("system_menus");

			builder.Property(e => e.Id)
				.HasColumnName("id");
			builder.Property(e => e.IsParent).HasColumnName("is_parent");
			builder.Property(e => e.IsSubParent).HasColumnName("is_sub_parent");
			builder.Property(e => e.MenuIcon)
				.HasMaxLength(50)
				.HasColumnName("menu_icon");
			builder.Property(e => e.MenuName)
				.IsRequired()
				.HasMaxLength(50)
				.HasColumnName("menu_name");
			builder.Property(e => e.MenuPath)
				.IsRequired()
				.HasMaxLength(50)
				.HasColumnName("menu_path");
			builder.Property(e => e.OrderBy).HasColumnName("order_by");
			builder.Property(e => e.ParentId).HasColumnName("parent_id");
			builder.Property(e => e.UserAccessTypeId).HasColumnName("user_access_type_id");

			builder.HasOne(d => d.UserAccessType).WithMany(p => p.SystemMenus)
				.HasForeignKey(d => d.UserAccessTypeId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_system_menus_user_access_types");
		}
	}
}
