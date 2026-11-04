using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RestaurantDATA.Migrations
{
    /// <inheritdoc />
    public partial class Saas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Restaurants",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    OwnerUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizations_ApplicationUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    MonthlyPriceUsd = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxLocations = table.Column<int>(type: "integer", nullable: false),
                    MaxEmployees = table.Column<int>(type: "integer", nullable: false),
                    ReservationsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    BasicAnalyticsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AdvancedAnalyticsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    QrMenuEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    KitchenDisplayEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    InventoryEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MultiBranchAnalyticsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TrialEndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CurrentPeriodEndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionPlans_PlanCode",
                        column: x => x.PlanCode,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Code", "AdvancedAnalyticsEnabled", "BasicAnalyticsEnabled", "InventoryEnabled", "IsActive", "KitchenDisplayEnabled", "MaxEmployees", "MaxLocations", "MonthlyPriceUsd", "MultiBranchAnalyticsEnabled", "Name", "QrMenuEnabled", "ReservationsEnabled" },
                values: new object[,]
                {
                    { "business", true, true, true, true, true, 60, 5, 179.00m, true, "Business", true, true },
                    { "pro", true, true, false, true, true, 20, 2, 79.99m, false, "Pro", true, true },
                    { "starter", false, true, false, true, false, 10, 1, 39.99m, false, "Starter", false, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_OrganizationId",
                table: "Restaurants",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_OwnerUserId",
                table: "Organizations",
                column: "OwnerUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_OrganizationId",
                table: "Subscriptions",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanCode",
                table: "Subscriptions",
                column: "PlanCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_Organizations_OrganizationId",
                table: "Restaurants",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_Organizations_OrganizationId",
                table: "Restaurants");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_OrganizationId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Restaurants");
        }
    }
}
