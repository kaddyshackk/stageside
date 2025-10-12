using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComedyPull.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateDataSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BronzeRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    BatchId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    ContentHash = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTimeOffset(new DateTime(2025, 10, 12, 2, 58, 26, 611, DateTimeKind.Unspecified).AddTicks(6001), new TimeSpan(0, 0, 0, 0, 0))),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTimeOffset(new DateTime(2025, 10, 12, 2, 58, 26, 611, DateTimeKind.Unspecified).AddTicks(6521), new TimeSpan(0, 0, 0, 0, 0))),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BronzeRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BronzeRecords");
        }
    }
}
