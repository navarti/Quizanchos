using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddedGameSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feature");

            migrationBuilder.AddColumn<int>(
                name: "FeatureType",
                table: "QuizCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FeatureAbstract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    Value = table.Column<float>(type: "real", nullable: true),
                    FeatureInt_Value = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureAbstract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureAbstract_QuizCategories_QuizCategoryId",
                        column: x => x.QuizCategoryId,
                        principalTable: "QuizCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeatureAbstract_QuizEntities_QuizEntityId",
                        column: x => x.QuizEntityId,
                        principalTable: "QuizEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SingleGameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CardsCount = table.Column<int>(type: "int", nullable: false),
                    CurrentCardIndex = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    GameLevel = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QuizCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleGameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SingleGameSessions_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SingleGameSessions_QuizCategories_QuizCategoryId",
                        column: x => x.QuizCategoryId,
                        principalTable: "QuizCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizCardAbstract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardIndex = table.Column<int>(type: "int", nullable: false),
                    CorrectOption = table.Column<int>(type: "int", nullable: false),
                    OptionPicked = table.Column<int>(type: "int", nullable: false),
                    SingleGameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    Option1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Option2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuizCardInt_Option1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuizCardInt_Option2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizCardAbstract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizCardAbstract_FeatureAbstract_Option1Id",
                        column: x => x.Option1Id,
                        principalTable: "FeatureAbstract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuizCardAbstract_FeatureAbstract_Option2Id",
                        column: x => x.Option2Id,
                        principalTable: "FeatureAbstract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuizCardAbstract_FeatureAbstract_QuizCardInt_Option1Id",
                        column: x => x.QuizCardInt_Option1Id,
                        principalTable: "FeatureAbstract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuizCardAbstract_FeatureAbstract_QuizCardInt_Option2Id",
                        column: x => x.QuizCardInt_Option2Id,
                        principalTable: "FeatureAbstract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuizCardAbstract_SingleGameSessions_SingleGameSessionId",
                        column: x => x.SingleGameSessionId,
                        principalTable: "SingleGameSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureAbstract_QuizCategoryId",
                table: "FeatureAbstract",
                column: "QuizCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureAbstract_QuizEntityId",
                table: "FeatureAbstract",
                column: "QuizEntityId");

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

            migrationBuilder.CreateIndex(
                name: "IX_QuizCardAbstract_SingleGameSessionId",
                table: "QuizCardAbstract",
                column: "SingleGameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleGameSessions_ApplicationUserId",
                table: "SingleGameSessions",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleGameSessions_QuizCategoryId",
                table: "SingleGameSessions",
                column: "QuizCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizCardAbstract");

            migrationBuilder.DropTable(
                name: "FeatureAbstract");

            migrationBuilder.DropTable(
                name: "SingleGameSessions");

            migrationBuilder.DropColumn(
                name: "FeatureType",
                table: "QuizCategories");

            migrationBuilder.CreateTable(
                name: "Feature",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Value = table.Column<float>(type: "real", nullable: true),
                    FeatureInt_Value = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feature_QuizCategories_QuizCategoryId",
                        column: x => x.QuizCategoryId,
                        principalTable: "QuizCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feature_QuizEntities_QuizEntityId",
                        column: x => x.QuizEntityId,
                        principalTable: "QuizEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feature_QuizCategoryId",
                table: "Feature",
                column: "QuizCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Feature_QuizEntityId",
                table: "Feature",
                column: "QuizEntityId");
        }
    }
}
