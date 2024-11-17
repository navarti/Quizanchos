using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddedQuizCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feature_QuizCategories_QuizCategoryId",
                table: "Feature");

            migrationBuilder.DropForeignKey(
                name: "FK_Feature_QuizEntities_QuizEntityId",
                table: "Feature");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Feature",
                table: "Feature");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Feature");

            migrationBuilder.DropColumn(
                name: "FeatureInt_Value",
                table: "Feature");

            migrationBuilder.RenameTable(
                name: "Feature",
                newName: "FeatureInts");

            migrationBuilder.RenameColumn(
                name: "QuestionsCount",
                table: "SingleGameSessions",
                newName: "GameLevel");

            migrationBuilder.RenameColumn(
                name: "CurrentQuestionIndex",
                table: "SingleGameSessions",
                newName: "CurrentCardIndex");

            migrationBuilder.RenameIndex(
                name: "IX_Feature_QuizEntityId",
                table: "FeatureInts",
                newName: "IX_FeatureInts_QuizEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Feature_QuizCategoryId",
                table: "FeatureInts",
                newName: "IX_FeatureInts_QuizCategoryId");

            migrationBuilder.AddColumn<int>(
                name: "CardsCount",
                table: "SingleGameSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FeatureType",
                table: "QuizCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Value",
                table: "FeatureInts",
                type: "int",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FeatureInts",
                table: "FeatureInts",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FeatureFloats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<float>(type: "real", nullable: true),
                    QuizCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFloats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureFloats_QuizCategories_QuizCategoryId",
                        column: x => x.QuizCategoryId,
                        principalTable: "QuizCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeatureFloats_QuizEntities_QuizEntityId",
                        column: x => x.QuizEntityId,
                        principalTable: "QuizEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFloats_QuizCategoryId",
                table: "FeatureFloats",
                column: "QuizCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFloats_QuizEntityId",
                table: "FeatureFloats",
                column: "QuizEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureInts_QuizCategories_QuizCategoryId",
                table: "FeatureInts",
                column: "QuizCategoryId",
                principalTable: "QuizCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureInts_QuizEntities_QuizEntityId",
                table: "FeatureInts",
                column: "QuizEntityId",
                principalTable: "QuizEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeatureInts_QuizCategories_QuizCategoryId",
                table: "FeatureInts");

            migrationBuilder.DropForeignKey(
                name: "FK_FeatureInts_QuizEntities_QuizEntityId",
                table: "FeatureInts");

            migrationBuilder.DropTable(
                name: "FeatureFloats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FeatureInts",
                table: "FeatureInts");

            migrationBuilder.DropColumn(
                name: "CardsCount",
                table: "SingleGameSessions");

            migrationBuilder.DropColumn(
                name: "FeatureType",
                table: "QuizCategories");

            migrationBuilder.RenameTable(
                name: "FeatureInts",
                newName: "Feature");

            migrationBuilder.RenameColumn(
                name: "GameLevel",
                table: "SingleGameSessions",
                newName: "QuestionsCount");

            migrationBuilder.RenameColumn(
                name: "CurrentCardIndex",
                table: "SingleGameSessions",
                newName: "CurrentQuestionIndex");

            migrationBuilder.RenameIndex(
                name: "IX_FeatureInts_QuizEntityId",
                table: "Feature",
                newName: "IX_Feature_QuizEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_FeatureInts_QuizCategoryId",
                table: "Feature",
                newName: "IX_Feature_QuizCategoryId");

            migrationBuilder.AlterColumn<float>(
                name: "Value",
                table: "Feature",
                type: "real",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Feature",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FeatureInt_Value",
                table: "Feature",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Feature",
                table: "Feature",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feature_QuizCategories_QuizCategoryId",
                table: "Feature",
                column: "QuizCategoryId",
                principalTable: "QuizCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feature_QuizEntities_QuizEntityId",
                table: "Feature",
                column: "QuizEntityId",
                principalTable: "QuizEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
