using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class DbRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game2048SessionStates_GameSessions_GameSessionId",
                table: "Game2048SessionStates");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizGameSessionStates_GameSessions_GameSessionId",
                table: "QuizGameSessionStates");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizGameSessionStates_QuizCategories_QuizCategoryId",
                table: "QuizGameSessionStates");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizMultiplayerSessionStates_GameSessions_GameSessionId",
                table: "QuizMultiplayerSessionStates");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizSessionCards_QuizGameSessionStates_QuizGameSessionStateId",
                table: "QuizSessionCards");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizSessionPlayerScores_QuizGameSessionStates_QuizGameSessionStateId",
                table: "QuizSessionPlayerScores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuizMultiplayerSessionStates",
                table: "QuizMultiplayerSessionStates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuizGameSessionStates",
                table: "QuizGameSessionStates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Game2048SessionStates",
                table: "Game2048SessionStates");

            migrationBuilder.RenameTable(
                name: "QuizMultiplayerSessionStates",
                newName: "QuizMultiplayerSessionState");

            migrationBuilder.RenameTable(
                name: "QuizGameSessionStates",
                newName: "QuizGameSessionState");

            migrationBuilder.RenameTable(
                name: "Game2048SessionStates",
                newName: "Game2048SessionState");

            migrationBuilder.RenameIndex(
                name: "IX_QuizMultiplayerSessionStates_GameSessionId",
                table: "QuizMultiplayerSessionState",
                newName: "IX_QuizMultiplayerSessionState_GameSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizGameSessionStates_QuizCategoryId",
                table: "QuizGameSessionState",
                newName: "IX_QuizGameSessionState_QuizCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizGameSessionStates_GameSessionId",
                table: "QuizGameSessionState",
                newName: "IX_QuizGameSessionState_GameSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Game2048SessionStates_GameSessionId",
                table: "Game2048SessionState",
                newName: "IX_Game2048SessionState_GameSessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuizMultiplayerSessionState",
                table: "QuizMultiplayerSessionState",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuizGameSessionState",
                table: "QuizGameSessionState",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Game2048SessionState",
                table: "Game2048SessionState",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GameSessionStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinigameType = table.Column<int>(type: "int", nullable: false),
                    StateJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessionStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessionStates_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameSessionStates_GameSessionId",
                table: "GameSessionStates",
                column: "GameSessionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Game2048SessionState_GameSessions_GameSessionId",
                table: "Game2048SessionState",
                column: "GameSessionId",
                principalTable: "GameSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizGameSessionState_GameSessions_GameSessionId",
                table: "QuizGameSessionState",
                column: "GameSessionId",
                principalTable: "GameSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizGameSessionState_QuizCategories_QuizCategoryId",
                table: "QuizGameSessionState",
                column: "QuizCategoryId",
                principalTable: "QuizCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizMultiplayerSessionState_GameSessions_GameSessionId",
                table: "QuizMultiplayerSessionState",
                column: "GameSessionId",
                principalTable: "GameSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSessionCards_QuizGameSessionState_QuizGameSessionStateId",
                table: "QuizSessionCards",
                column: "QuizGameSessionStateId",
                principalTable: "QuizGameSessionState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSessionPlayerScores_QuizGameSessionState_QuizGameSessionStateId",
                table: "QuizSessionPlayerScores",
                column: "QuizGameSessionStateId",
                principalTable: "QuizGameSessionState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game2048SessionState_GameSessions_GameSessionId",
                table: "Game2048SessionState");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizGameSessionState_GameSessions_GameSessionId",
                table: "QuizGameSessionState");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizGameSessionState_QuizCategories_QuizCategoryId",
                table: "QuizGameSessionState");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizMultiplayerSessionState_GameSessions_GameSessionId",
                table: "QuizMultiplayerSessionState");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizSessionCards_QuizGameSessionState_QuizGameSessionStateId",
                table: "QuizSessionCards");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizSessionPlayerScores_QuizGameSessionState_QuizGameSessionStateId",
                table: "QuizSessionPlayerScores");

            migrationBuilder.DropTable(
                name: "GameSessionStates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuizMultiplayerSessionState",
                table: "QuizMultiplayerSessionState");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuizGameSessionState",
                table: "QuizGameSessionState");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Game2048SessionState",
                table: "Game2048SessionState");

            migrationBuilder.RenameTable(
                name: "QuizMultiplayerSessionState",
                newName: "QuizMultiplayerSessionStates");

            migrationBuilder.RenameTable(
                name: "QuizGameSessionState",
                newName: "QuizGameSessionStates");

            migrationBuilder.RenameTable(
                name: "Game2048SessionState",
                newName: "Game2048SessionStates");

            migrationBuilder.RenameIndex(
                name: "IX_QuizMultiplayerSessionState_GameSessionId",
                table: "QuizMultiplayerSessionStates",
                newName: "IX_QuizMultiplayerSessionStates_GameSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizGameSessionState_QuizCategoryId",
                table: "QuizGameSessionStates",
                newName: "IX_QuizGameSessionStates_QuizCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizGameSessionState_GameSessionId",
                table: "QuizGameSessionStates",
                newName: "IX_QuizGameSessionStates_GameSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Game2048SessionState_GameSessionId",
                table: "Game2048SessionStates",
                newName: "IX_Game2048SessionStates_GameSessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuizMultiplayerSessionStates",
                table: "QuizMultiplayerSessionStates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuizGameSessionStates",
                table: "QuizGameSessionStates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Game2048SessionStates",
                table: "Game2048SessionStates",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Game2048SessionStates_GameSessions_GameSessionId",
                table: "Game2048SessionStates",
                column: "GameSessionId",
                principalTable: "GameSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizGameSessionStates_GameSessions_GameSessionId",
                table: "QuizGameSessionStates",
                column: "GameSessionId",
                principalTable: "GameSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizGameSessionStates_QuizCategories_QuizCategoryId",
                table: "QuizGameSessionStates",
                column: "QuizCategoryId",
                principalTable: "QuizCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizMultiplayerSessionStates_GameSessions_GameSessionId",
                table: "QuizMultiplayerSessionStates",
                column: "GameSessionId",
                principalTable: "GameSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSessionCards_QuizGameSessionStates_QuizGameSessionStateId",
                table: "QuizSessionCards",
                column: "QuizGameSessionStateId",
                principalTable: "QuizGameSessionStates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSessionPlayerScores_QuizGameSessionStates_QuizGameSessionStateId",
                table: "QuizSessionPlayerScores",
                column: "QuizGameSessionStateId",
                principalTable: "QuizGameSessionStates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
