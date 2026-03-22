using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    PriceCoins = table.Column<int>(type: "int", nullable: false),
                    IsFree = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketItems", x => x.Id);
                    table.CheckConstraint("CK_MarketItems_PriceCoins_NonNegative", "[PriceCoins] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "UserOwnedItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MarketItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchasedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_MarketItems_Type_Name",
                table: "MarketItems",
                columns: new[] { "Type", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedItems_ApplicationUserId_MarketItemId",
                table: "UserOwnedItems",
                columns: new[] { "ApplicationUserId", "MarketItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedItems_MarketItemId",
                table: "UserOwnedItems",
                column: "MarketItemId");

            migrationBuilder.InsertData(
                table: "MarketItems",
                columns: new[] { "Id", "ImageUrl", "IsActive", "IsFree", "Name", "PriceCoins", "Type" },
                values: new object[,]
                {
                    { new Guid("7d11a1f8-9be0-4b2b-9a03-196baa4b5231"), "https://twemoji.maxcdn.com/v/latest/svg/1f44d.svg", true, true, "Thumbs Up", 0, 1 },
                    { new Guid("e5e8229a-5b86-470c-beaf-86b3a7a89e40"), "https://twemoji.maxcdn.com/v/latest/svg/1f525.svg", true, false, "Fire", 120, 1 },
                    { new Guid("90b2a880-ffb8-4a84-a2d7-5a8f3797b5d4"), "https://twemoji.maxcdn.com/v/latest/svg/1f389.svg", true, false, "Party", 200, 1 },
                    { new Guid("90cac16a-29cf-47af-a713-1695d16f27c4"), "https://twemoji.maxcdn.com/v/latest/svg/1f3c6.svg", true, false, "Trophy", 300, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOwnedItems");

            migrationBuilder.DropTable(
                name: "MarketItems");
        }
    }
}
