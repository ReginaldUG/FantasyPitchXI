using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyPitchXI.Migrations
{
    /// <inheritdoc />
    public partial class AddFantasyTeamLineup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FantasyTeamLineups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FantasyTeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Gameweek = table.Column<int>(type: "INTEGER", nullable: false),
                    IsStarting = table.Column<bool>(type: "INTEGER", nullable: false),
                    BenchOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCaptain = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsViceCaptain = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyTeamLineups", x => x.Id);
                    table.CheckConstraint("CK_Captain_Vice", "NOT ([IsCaptain] = 1 AND [IsViceCaptain] = 1)");
                    table.ForeignKey(
                        name: "FK_FantasyTeamLineups_FantasyTeamPlayer_FantasyTeamId_PlayerId",
                        columns: x => new { x.FantasyTeamId, x.PlayerId },
                        principalTable: "FantasyTeamPlayer",
                        principalColumns: new[] { "FantasyTeamId", "PlayerId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeamLineups_FantasyTeamId_PlayerId_Gameweek",
                table: "FantasyTeamLineups",
                columns: new[] { "FantasyTeamId", "PlayerId", "Gameweek" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FantasyTeamLineups");
        }
    }
}
