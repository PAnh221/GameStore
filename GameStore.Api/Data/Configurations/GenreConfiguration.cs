using GameStore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameStore.Api.Data.Configurations;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
	public void Configure(EntityTypeBuilder<Genre> builder)
	{
		builder.Property(genre => genre.Name).HasMaxLength(50);
		builder.HasData(
			new { Id = 1, Name = "Fighting" },
			new { Id = 2, Name = "Roleplaying" },
			new { Id = 3, Name = "Sports" },
			new { Id = 4, Name = "Racing" },
			new { Id = 5, Name = "Kids and Family" }
		);
	}
}
