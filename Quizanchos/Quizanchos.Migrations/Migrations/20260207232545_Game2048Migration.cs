using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Game2048Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Game2048SessionStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    BoardJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    BestTile = table.Column<int>(type: "int", nullable: false),
                    MoveCount = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game2048SessionStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Game2048SessionStates_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Game2048SessionStates_GameSessionId",
                table: "Game2048SessionStates",
                column: "GameSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Game2048SessionStates");
        }
    }
}
