using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantDATA.Migrations
{
    /// <inheritdoc />
    public partial class table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Tables",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tables",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tables",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Zone",
                table: "Tables",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Zone",
                table: "Tables");
        }
    }
}
