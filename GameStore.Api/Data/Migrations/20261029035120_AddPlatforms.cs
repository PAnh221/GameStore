using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GameStore.Api.Data.Migrations
{
	/// <inheritdoc />
	public partial class AddPlatforms : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Platforms",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Platforms", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "GamePlatform",
				columns: table => new
				{
					GamesId = table.Column<int>(type: "INTEGER", nullable: false),
					PlatformsId = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GamePlatform", x => new { x.GamesId, x.PlatformsId });
					table.ForeignKey(
						name: "FK_GamePlatform_Games_GamesId",
						column: x => x.GamesId,
						principalTable: "Games",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_GamePlatform_Platforms_PlatformsId",
						column: x => x.PlatformsId,
						principalTable: "Platforms",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.InsertData(
				table: "Platforms",
				columns: new[] { "Id", "Name" },
				values: new object[,]
				{
					{ 1, "PC" },
					{ 2, "PlayStation 5" },
					{ 3, "Xbox Series X|S" },
					{ 4, "Nintendo Switch" },
					{ 5, "Mobile" }
				});

			migrationBuilder.CreateIndex(
				name: "IX_GamePlatform_PlatformsId",
				table: "GamePlatform",
				column: "PlatformsId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "GamePlatform");

			migrationBuilder.DropTable(
				name: "Platforms");
		}
	}
}
