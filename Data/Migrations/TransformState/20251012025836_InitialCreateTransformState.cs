using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComedyPull.Data.Migrations.TransformState
{
    /// <inheritdoc />
    public partial class InitialCreateTransformState : Migration
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTimeOffset(new DateTime(2025, 10, 12, 2, 58, 36, 454, DateTimeKind.Unspecified).AddTicks(6651), new TimeSpan(0, 0, 0, 0, 0))),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTimeOffset(new DateTime(2025, 10, 12, 2, 58, 36, 454, DateTimeKind.Unspecified).AddTicks(7224), new TimeSpan(0, 0, 0, 0, 0))),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BronzeRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SilverRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    BatchId = table.Column<string>(type: "text", nullable: false),
                    BronzeRecordId = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    ContentHash = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTimeOffset(new DateTime(2025, 10, 12, 2, 58, 36, 454, DateTimeKind.Unspecified).AddTicks(9346), new TimeSpan(0, 0, 0, 0, 0))),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTimeOffset(new DateTime(2025, 10, 12, 2, 58, 36, 454, DateTimeKind.Unspecified).AddTicks(9862), new TimeSpan(0, 0, 0, 0, 0))),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SilverRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BronzeRecords");

            migrationBuilder.DropTable(
                name: "SilverRecords");
        }
    }
}
