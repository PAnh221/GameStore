using GameStore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameStore.Api.Data.Configurations;

public class PlatformConfiguration : IEntityTypeConfiguration<Platform>
{
	public void Configure(EntityTypeBuilder<Platform> builder)
	{
		builder.Property(platform => platform.Name).HasMaxLength(50);
		builder.HasData(
			new { Id = 1, Name = "PC" },
			new { Id = 2, Name = "PlayStation 5" },
			new { Id = 3, Name = "Xbox Series X|S" },
			new { Id = 4, Name = "Nintendo Switch" },
			new { Id = 5, Name = "Mobile" }
		);
	}
}
