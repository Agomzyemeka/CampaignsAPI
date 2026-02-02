using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignsAPI.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "User"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@campaigns.com", "System Administrator", true, null, "$2a$11$3LZtFmqJ8VKhCqXqYQJ3yO.rqtVEVT2hOxO3FxYZCPNvJhb8nHp7y", "Admin", "admin" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[] { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "demo@campaigns.com", "Demo User", true, null, "$2a$11$3LZtFmqJ8VKhCqXqYQJ3yO.rqtVEVT2hOxO3FxYZCPNvJhb8nHp7y", "User", "demo" });

            migrationBuilder.InsertData(
                table: "Campaigns",
                columns: new[] { "Id", "Budget", "CreatedAt", "CreatedBy", "Description", "EndDate", "Name", "StartDate", "Status", "UpdatedAt" },
                values: new object[] { 1, 50000.00m, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Major summer promotion campaign with 30% discounts", new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Summer Sale 2024", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, null });

            migrationBuilder.InsertData(
                table: "Campaigns",
                columns: new[] { "Id", "Budget", "CreatedAt", "CreatedBy", "Description", "EndDate", "Name", "StartDate", "Status", "UpdatedAt" },
                values: new object[] { 2, 75000.00m, new DateTime(2024, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "End of year holiday promotions and special offers", new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Holiday Season Campaign", new DateTime(2024, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null });

            migrationBuilder.InsertData(
                table: "Campaigns",
                columns: new[] { "Id", "Budget", "CreatedAt", "CreatedBy", "Description", "EndDate", "Name", "StartDate", "Status", "UpdatedAt" },
                values: new object[] { 3, 100000.00m, new DateTime(2024, 12, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Launch campaign for new product line", new DateTime(2025, 5, 31, 0, 0, 0, 0, DateTimeKind.Utc), "New Product Launch", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, null });

            migrationBuilder.InsertData(
                table: "Campaigns",
                columns: new[] { "Id", "Budget", "CreatedAt", "CreatedBy", "Description", "EndDate", "Name", "StartDate", "Status", "UpdatedAt" },
                values: new object[] { 4, 25000.00m, new DateTime(2024, 12, 20, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Increase social media engagement through targeted ads", new DateTime(2025, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Social Media Boost", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, null });

            migrationBuilder.InsertData(
                table: "Campaigns",
                columns: new[] { "Id", "Budget", "CreatedAt", "CreatedBy", "Description", "EndDate", "Name", "StartDate", "Status", "UpdatedAt" },
                values: new object[] { 5, 15000.00m, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Targeted email campaign for customer retention", new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Email Marketing Campaign", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedBy",
                table: "Campaigns",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_DateRange",
                table: "Campaigns",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_IsDeleted",
                table: "Campaigns",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Name",
                table: "Campaigns",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Status",
                table: "Campaigns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_Unique",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
