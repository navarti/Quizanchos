using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Quizanchos.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: false),
                    Coins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PremiumUntilUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PriceCoins = table.Column<int>(type: "integer", nullable: false),
                    IsFree = table.Column<bool>(type: "boolean", nullable: false),
                    DurationMonths = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketItems", x => x.Id);
                    table.CheckConstraint("CK_MarketItems_DurationMonths_ByType", "(\"Type\" <> 2 AND \"DurationMonths\" IS NULL) OR (\"Type\" = 2 AND \"DurationMonths\" IS NOT NULL AND \"DurationMonths\" > 0)");
                    table.CheckConstraint("CK_MarketItems_PriceCoins_NonNegative", "\"PriceCoins\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "QuizCardAbstract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CardIndex = table.Column<int>(type: "integer", nullable: false),
                    CorrectOption = table.Column<int>(type: "integer", nullable: false),
                    OptionPicked = table.Column<int>(type: "integer", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Options = table.Column<string>(type: "text", nullable: true),
                    QuizCardInt_Options = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizCardAbstract", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FeatureType = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    AuthorName = table.Column<string>(type: "text", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    QuestionToDisplay = table.Column<string>(type: "text", nullable: false),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
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
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
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
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MinigameType = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                    WinnerId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                name: "TopUpOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CoinsToCredit = table.Column<int>(type: "integer", nullable: false),
                    AmountUSDT = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Network = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    BinanceTxId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopUpOrders", x => x.Id);
                    table.CheckConstraint("CK_TopUpOrders_AmountUSDT_Positive", "\"AmountUSDT\" > 0");
                    table.CheckConstraint("CK_TopUpOrders_CoinsToCredit_Positive", "\"CoinsToCredit\" > 0");
                    table.ForeignKey(
                        name: "FK_TopUpOrders_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMinigameScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    MinigameType = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMinigameScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMinigameScores_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOwnedItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    MarketItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOwnedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOwnedItems_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserOwnedItems_MarketItems_MarketItemId",
                        column: x => x.MarketItemId,
                        principalTable: "MarketItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeatureAbstract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Value = table.Column<float>(type: "real", nullable: true),
                    FeatureInt_Value = table.Column<int>(type: "integer", nullable: true)
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
                name: "Game2048SessionState",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    BoardJson = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    BestTile = table.Column<int>(type: "integer", nullable: false),
                    MoveCount = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game2048SessionState", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Game2048SessionState_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameSessionPlayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                name: "GameSessionStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinigameType = table.Column<int>(type: "integer", nullable: false),
                    StateJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "QuizGameSessionState",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    GameLevel = table.Column<int>(type: "integer", nullable: false),
                    SecondsPerCard = table.Column<int>(type: "integer", nullable: false),
                    OptionCount = table.Column<int>(type: "integer", nullable: false),
                    TotalCards = table.Column<int>(type: "integer", nullable: false),
                    CurrentCardIndex = table.Column<int>(type: "integer", nullable: false),
                    IsTerminatedByTime = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizGameSessionState", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizGameSessionState_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizGameSessionState_QuizCategories_QuizCategoryId",
                        column: x => x.QuizCategoryId,
                        principalTable: "QuizCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizMultiplayerSessionState",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StateJson = table.Column<string>(type: "text", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizMultiplayerSessionState", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizMultiplayerSessionState_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizSessionCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizGameSessionStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CardIndex = table.Column<int>(type: "integer", nullable: false),
                    CorrectOption = table.Column<int>(type: "integer", nullable: false),
                    OptionPicked = table.Column<int>(type: "integer", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EntityIdsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EntityNamesJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    OptionValuesJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSessionCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSessionCards_QuizGameSessionState_QuizGameSessionStateId",
                        column: x => x.QuizGameSessionStateId,
                        principalTable: "QuizGameSessionState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizSessionPlayerScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizGameSessionStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
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
                        name: "FK_QuizSessionPlayerScores_QuizGameSessionState_QuizGameSessio~",
                        column: x => x.QuizGameSessionStateId,
                        principalTable: "QuizGameSessionState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizSessionCardAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizSessionCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    OptionPicked = table.Column<int>(type: "integer", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                unique: true);

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
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureAbstract_QuizCategoryId",
                table: "FeatureAbstract",
                column: "QuizCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureAbstract_QuizEntityId",
                table: "FeatureAbstract",
                column: "QuizEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Game2048SessionState_GameSessionId",
                table: "Game2048SessionState",
                column: "GameSessionId");

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
                name: "IX_GameSessionStates_GameSessionId",
                table: "GameSessionStates",
                column: "GameSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketItems_Type_Name",
                table: "MarketItems",
                columns: new[] { "Type", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizGameSessionState_GameSessionId",
                table: "QuizGameSessionState",
                column: "GameSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizGameSessionState_QuizCategoryId",
                table: "QuizGameSessionState",
                column: "QuizCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizMultiplayerSessionState_GameSessionId",
                table: "QuizMultiplayerSessionState",
                column: "GameSessionId",
                unique: true);

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
                name: "IX_QuizSessionPlayerScores_QuizGameSessionStateId_ApplicationU~",
                table: "QuizSessionPlayerScores",
                columns: new[] { "QuizGameSessionStateId", "ApplicationUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopUpOrders_AmountUSDT_Network_Status",
                table: "TopUpOrders",
                columns: new[] { "AmountUSDT", "Network", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TopUpOrders_ApplicationUserId",
                table: "TopUpOrders",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMinigameScores_ApplicationUserId",
                table: "UserMinigameScores",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedItems_ApplicationUserId_MarketItemId",
                table: "UserOwnedItems",
                columns: new[] { "ApplicationUserId", "MarketItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedItems_MarketItemId",
                table: "UserOwnedItems",
                column: "MarketItemId");
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
                name: "Game2048SessionState");

            migrationBuilder.DropTable(
                name: "GameSessionPlayers");

            migrationBuilder.DropTable(
                name: "GameSessionStates");

            migrationBuilder.DropTable(
                name: "QuizCardAbstract");

            migrationBuilder.DropTable(
                name: "QuizMultiplayerSessionState");

            migrationBuilder.DropTable(
                name: "QuizSessionCardAnswers");

            migrationBuilder.DropTable(
                name: "QuizSessionPlayerScores");

            migrationBuilder.DropTable(
                name: "TopUpOrders");

            migrationBuilder.DropTable(
                name: "UserMinigameScores");

            migrationBuilder.DropTable(
                name: "UserOwnedItems");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "QuizEntities");

            migrationBuilder.DropTable(
                name: "QuizSessionCards");

            migrationBuilder.DropTable(
                name: "MarketItems");

            migrationBuilder.DropTable(
                name: "QuizGameSessionState");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "QuizCategories");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
