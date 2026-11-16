using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantDATA.Migrations
{
    /// <inheritdoc />
    public partial class pin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffPinCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    PinHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PepperKeyVersion = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    FailedAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LockedUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPinCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffPinCredentials_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPinCredentials_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffSecurityAuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrustedStaffDeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Action = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffSecurityAuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffSecurityAuditEvents_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffSecurityAuditEvents_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrustedStaffDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CredentialHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EnrolledByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustedStaffDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrustedStaffDevices_ApplicationUsers_EnrolledByUserId",
                        column: x => x.EnrolledByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrustedStaffDevices_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrustedStaffDevices_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffDeviceSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrustedStaffDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffDeviceSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffDeviceSessions_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffDeviceSessions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffDeviceSessions_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffDeviceSessions_TrustedStaffDevices_TrustedStaffDeviceId",
                        column: x => x.TrustedStaffDeviceId,
                        principalTable: "TrustedStaffDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffDeviceSessions_OrganizationId",
                table: "StaffDeviceSessions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffDeviceSessions_RefreshTokenHash",
                table: "StaffDeviceSessions",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffDeviceSessions_RestaurantId",
                table: "StaffDeviceSessions",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffDeviceSessions_TrustedStaffDeviceId_ExpiresAt",
                table: "StaffDeviceSessions",
                columns: new[] { "TrustedStaffDeviceId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffDeviceSessions_UserId_RevokedAt",
                table: "StaffDeviceSessions",
                columns: new[] { "UserId", "RevokedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffPinCredentials_OrganizationId_UserId",
                table: "StaffPinCredentials",
                columns: new[] { "OrganizationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffPinCredentials_UserId",
                table: "StaffPinCredentials",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffSecurityAuditEvents_OrganizationId_OccurredAt",
                table: "StaffSecurityAuditEvents",
                columns: new[] { "OrganizationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffSecurityAuditEvents_RestaurantId",
                table: "StaffSecurityAuditEvents",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffSecurityAuditEvents_TrustedStaffDeviceId_OccurredAt",
                table: "StaffSecurityAuditEvents",
                columns: new[] { "TrustedStaffDeviceId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrustedStaffDevices_CredentialHash",
                table: "TrustedStaffDevices",
                column: "CredentialHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrustedStaffDevices_EnrolledByUserId",
                table: "TrustedStaffDevices",
                column: "EnrolledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustedStaffDevices_OrganizationId_RestaurantId",
                table: "TrustedStaffDevices",
                columns: new[] { "OrganizationId", "RestaurantId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrustedStaffDevices_RestaurantId",
                table: "TrustedStaffDevices",
                column: "RestaurantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffDeviceSessions");

            migrationBuilder.DropTable(
                name: "StaffPinCredentials");

            migrationBuilder.DropTable(
                name: "StaffSecurityAuditEvents");

            migrationBuilder.DropTable(
                name: "TrustedStaffDevices");
        }
    }
}
