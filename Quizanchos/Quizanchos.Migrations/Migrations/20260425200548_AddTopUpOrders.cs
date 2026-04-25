using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddTopUpOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TopUpOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CoinsToCredit = table.Column<int>(type: "int", nullable: false),
                    AmountUSDT = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Network = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BinanceTxId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopUpOrders", x => x.Id);
                    table.CheckConstraint("CK_TopUpOrders_AmountUSDT_Positive", "[AmountUSDT] > 0");
                    table.CheckConstraint("CK_TopUpOrders_CoinsToCredit_Positive", "[CoinsToCredit] > 0");
                    table.ForeignKey(
                        name: "FK_TopUpOrders_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopUpOrders_AmountUSDT_Network_Status",
                table: "TopUpOrders",
                columns: new[] { "AmountUSDT", "Network", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TopUpOrders_ApplicationUserId",
                table: "TopUpOrders",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopUpOrders");
        }
    }
}
