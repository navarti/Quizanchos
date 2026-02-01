using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialQuizDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizCardAbstract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardIndex = table.Column<int>(type: "int", nullable: false),
                    CorrectOption = table.Column<int>(type: "int", nullable: false),
                    OptionPicked = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuizCardInt_Options = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizCardAbstract", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeatureType = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuestionToDisplay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPremium = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinigameType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    WinnerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSession_ApplicationUser_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                });

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
                name: "GameSessionPlayer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessionPlayer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessionPlayer_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameSessionPlayer_GameSession_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizGameSessionStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameLevel = table.Column<int>(type: "int", nullable: false),
                    SecondsPerCard = table.Column<int>(type: "int", nullable: false),
                    OptionCount = table.Column<int>(type: "int", nullable: false),
                    TotalCards = table.Column<int>(type: "int", nullable: false),
                    CurrentCardIndex = table.Column<int>(type: "int", nullable: false),
                    IsTerminatedByTime = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizGameSessionStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizGameSessionStates_GameSession_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizGameSessionStates_QuizCategories_QuizCategoryId",
                        column: x => x.QuizCategoryId,
                        principalTable: "QuizCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizSessionCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizGameSessionStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardIndex = table.Column<int>(type: "int", nullable: false),
                    CorrectOption = table.Column<int>(type: "int", nullable: false),
                    OptionPicked = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntityIdsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    EntityNamesJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    OptionValuesJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSessionCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSessionCards_QuizGameSessionStates_QuizGameSessionStateId",
                        column: x => x.QuizGameSessionStateId,
                        principalTable: "QuizGameSessionStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizSessionPlayerScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizGameSessionStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSessionPlayerScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSessionPlayerScores_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizSessionPlayerScores_QuizGameSessionStates_QuizGameSessionStateId",
                        column: x => x.QuizGameSessionStateId,
                        principalTable: "QuizGameSessionStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizSessionCardAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizSessionCardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OptionPicked = table.Column<int>(type: "int", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSessionCardAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSessionCardAnswers_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizSessionCardAnswers_QuizSessionCards_QuizSessionCardId",
                        column: x => x.QuizSessionCardId,
                        principalTable: "QuizSessionCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_GameSession_WinnerId",
                table: "GameSession",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessionPlayer_ApplicationUserId",
                table: "GameSessionPlayer",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessionPlayer_GameSessionId",
                table: "GameSessionPlayer",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizGameSessionStates_GameSessionId",
                table: "QuizGameSessionStates",
                column: "GameSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizGameSessionStates_QuizCategoryId",
                table: "QuizGameSessionStates",
                column: "QuizCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionCardAnswers_ApplicationUserId",
                table: "QuizSessionCardAnswers",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionCardAnswers_QuizSessionCardId_ApplicationUserId",
                table: "QuizSessionCardAnswers",
                columns: new[] { "QuizSessionCardId", "ApplicationUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionCards_QuizGameSessionStateId_CardIndex",
                table: "QuizSessionCards",
                columns: new[] { "QuizGameSessionStateId", "CardIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionPlayerScores_ApplicationUserId",
                table: "QuizSessionPlayerScores",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionPlayerScores_QuizGameSessionStateId_ApplicationUserId",
                table: "QuizSessionPlayerScores",
                columns: new[] { "QuizGameSessionStateId", "ApplicationUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureAbstract");

            migrationBuilder.DropTable(
                name: "GameSessionPlayer");

            migrationBuilder.DropTable(
                name: "QuizCardAbstract");

            migrationBuilder.DropTable(
                name: "QuizSessionCardAnswers");

            migrationBuilder.DropTable(
                name: "QuizSessionPlayerScores");

            migrationBuilder.DropTable(
                name: "QuizEntities");

            migrationBuilder.DropTable(
                name: "QuizSessionCards");

            migrationBuilder.DropTable(
                name: "QuizGameSessionStates");

            migrationBuilder.DropTable(
                name: "GameSession");

            migrationBuilder.DropTable(
                name: "QuizCategories");

            migrationBuilder.DropTable(
                name: "ApplicationUser");
        }
    }
}
