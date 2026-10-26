using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantDATA.Migrations
{
    /// <inheritdoc />
    public partial class Restaurantsaas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "Tables",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "Reservations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "OrderItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "Categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantUsers",
                columns: table => new
                {
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantUsers", x => new { x.RestaurantId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RestaurantUsers_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantUsers_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tables_RestaurantId",
                table: "Tables",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_RestaurantId",
                table: "Reservations",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_RestaurantId",
                table: "Products",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RestaurantId",
                table: "Payments",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RestaurantId",
                table: "Orders",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_RestaurantId",
                table: "OrderItems",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_RestaurantId",
                table: "Categories",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Code",
                table: "Restaurants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Slug",
                table: "Restaurants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantUsers_UserId",
                table: "RestaurantUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Restaurants_RestaurantId",
                table: "Categories",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Restaurants_RestaurantId",
                table: "OrderItems",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Restaurants_RestaurantId",
                table: "Orders",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Restaurants_RestaurantId",
                table: "Payments",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Restaurants_RestaurantId",
                table: "Products",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Restaurants_RestaurantId",
                table: "Reservations",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_Restaurants_RestaurantId",
                table: "Tables",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Restaurants_RestaurantId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Restaurants_RestaurantId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Restaurants_RestaurantId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Restaurants_RestaurantId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Restaurants_RestaurantId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Restaurants_RestaurantId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_Restaurants_RestaurantId",
                table: "Tables");

            migrationBuilder.DropTable(
                name: "RestaurantUsers");

            migrationBuilder.DropTable(
                name: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Tables_RestaurantId",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_RestaurantId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Products_RestaurantId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Payments_RestaurantId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_RestaurantId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_RestaurantId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Categories_RestaurantId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Categories");
        }
    }
}
