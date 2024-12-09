using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixedDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizCardAbstract_SingleGameSessions_SingleGameSessionId",
                table: "QuizCardAbstract");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizCardAbstract_SingleGameSessions_SingleGameSessionId",
                table: "QuizCardAbstract",
                column: "SingleGameSessionId",
                principalTable: "SingleGameSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizCardAbstract_SingleGameSessions_SingleGameSessionId",
                table: "QuizCardAbstract");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizCardAbstract_SingleGameSessions_SingleGameSessionId",
                table: "QuizCardAbstract",
                column: "SingleGameSessionId",
                principalTable: "SingleGameSessions",
                principalColumn: "Id");
        }
    }
}
