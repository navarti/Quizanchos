using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
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
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessions_AspNetUsers_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "GameSessionPlayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessionPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessionPlayers_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameSessionPlayers_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
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
                        name: "FK_QuizGameSessionStates_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
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
                        name: "FK_QuizSessionPlayerScores_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
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
                        name: "FK_QuizSessionCardAnswers_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureAbstract_QuizCategoryId",
                table: "FeatureAbstract",
                column: "QuizCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureAbstract_QuizEntityId",
                table: "FeatureAbstract",
                column: "QuizEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessionPlayers_ApplicationUserId",
                table: "GameSessionPlayers",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessionPlayers_GameSessionId_ApplicationUserId",
                table: "GameSessionPlayers",
                columns: new[] { "GameSessionId", "ApplicationUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_WinnerId",
                table: "GameSessions",
                column: "WinnerId");

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
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "FeatureAbstract");

            migrationBuilder.DropTable(
                name: "GameSessionPlayers");

            migrationBuilder.DropTable(
                name: "QuizCardAbstract");

            migrationBuilder.DropTable(
                name: "QuizSessionCardAnswers");

            migrationBuilder.DropTable(
                name: "QuizSessionPlayerScores");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "QuizEntities");

            migrationBuilder.DropTable(
                name: "QuizSessionCards");

            migrationBuilder.DropTable(
                name: "QuizGameSessionStates");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "QuizCategories");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
