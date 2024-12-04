using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MultipleOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizCardAbstract_FeatureAbstract_Option1Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizCardAbstract_FeatureAbstract_Option2Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizCardAbstract_FeatureAbstract_QuizCardInt_Option1Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizCardAbstract_FeatureAbstract_QuizCardInt_Option2Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropIndex(
                name: "IX_QuizCardAbstract_Option1Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropIndex(
                name: "IX_QuizCardAbstract_Option2Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropIndex(
                name: "IX_QuizCardAbstract_QuizCardInt_Option1Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropIndex(
                name: "IX_QuizCardAbstract_QuizCardInt_Option2Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropColumn(
                name: "Option1Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropColumn(
                name: "Option2Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropColumn(
                name: "QuizCardInt_Option1Id",
                table: "QuizCardAbstract");

            migrationBuilder.DropColumn(
                name: "QuizCardInt_Option2Id",
                table: "QuizCardAbstract");

            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "QuizCardAbstract",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuizCardInt_Options",
                table: "QuizCardAbstract",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Options",
                table: "QuizCardAbstract");

            migrationBuilder.DropColumn(
                name: "QuizCardInt_Options",
                table: "QuizCardAbstract");

            migrationBuilder.AddColumn<Guid>(
                name: "Option1Id",
                table: "QuizCardAbstract",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Option2Id",
                table: "QuizCardAbstract",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QuizCardInt_Option1Id",
                table: "QuizCardAbstract",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QuizCardInt_Option2Id",
                table: "QuizCardAbstract",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizCardAbstract_Option1Id",
                table: "QuizCardAbstract",
                column: "Option1Id");

            migrationBuilder.CreateIndex(
                name: "IX_QuizCardAbstract_Option2Id",
                table: "QuizCardAbstract",
                column: "Option2Id");

            migrationBuilder.CreateIndex(
                name: "IX_QuizCardAbstract_QuizCardInt_Option1Id",
                table: "QuizCardAbstract",
                column: "QuizCardInt_Option1Id");

            migrationBuilder.CreateIndex(
                name: "IX_QuizCardAbstract_QuizCardInt_Option2Id",
                table: "QuizCardAbstract",
                column: "QuizCardInt_Option2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizCardAbstract_FeatureAbstract_Option1Id",
                table: "QuizCardAbstract",
                column: "Option1Id",
                principalTable: "FeatureAbstract",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizCardAbstract_FeatureAbstract_Option2Id",
                table: "QuizCardAbstract",
                column: "Option2Id",
                principalTable: "FeatureAbstract",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizCardAbstract_FeatureAbstract_QuizCardInt_Option1Id",
                table: "QuizCardAbstract",
                column: "QuizCardInt_Option1Id",
                principalTable: "FeatureAbstract",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizCardAbstract_FeatureAbstract_QuizCardInt_Option2Id",
                table: "QuizCardAbstract",
                column: "QuizCardInt_Option2Id",
                principalTable: "FeatureAbstract",
                principalColumn: "Id");
        }
    }
}
