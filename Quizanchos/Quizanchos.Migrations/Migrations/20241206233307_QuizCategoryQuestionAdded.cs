using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class QuizCategoryQuestionAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuestionToDisplay",
                table: "QuizCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionToDisplay",
                table: "QuizCategories");
        }
    }
}
