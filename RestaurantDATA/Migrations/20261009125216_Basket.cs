using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantDATA.Migrations
{
    /// <inheritdoc />
    public partial class Basket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Baskets",
                newName: "UnitPrice");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Baskets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Baskets");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "Baskets",
                newName: "Price");
        }
    }
}
