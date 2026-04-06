using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizanchos.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddPremiumSubscriptionDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMonths",
                table: "MarketItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PremiumUntilUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MarketItems_DurationMonths_ByType",
                table: "MarketItems",
                sql: "([Type] <> 2 AND [DurationMonths] IS NULL) OR ([Type] = 2 AND [DurationMonths] IS NOT NULL AND [DurationMonths] > 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MarketItems_DurationMonths_ByType",
                table: "MarketItems");

            migrationBuilder.DropColumn(
                name: "DurationMonths",
                table: "MarketItems");

            migrationBuilder.DropColumn(
                name: "PremiumUntilUtc",
                table: "AspNetUsers");
        }
    }
}
