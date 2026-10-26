using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationGuestContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "Reservations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestPhone",
                table: "Reservations",
                type: "character varying(25)",
                maxLength: 25,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestName",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GuestPhone",
                table: "Reservations");
        }
    }
}
