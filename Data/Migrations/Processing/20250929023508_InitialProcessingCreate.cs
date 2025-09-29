using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComedyPull.Data.Migrations.Processing
{
    /// <inheritdoc />
    public partial class InitialProcessingCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SourceRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    BatchId = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<int>(type: "integer", nullable: false),
                    RecordType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    RawData = table.Column<string>(type: "text", nullable: false),
                    ProcessedData = table.Column<string>(type: "text", nullable: true),
                    ContentHash = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTimeOffset(new DateTime(2025, 9, 29, 2, 35, 8, 342, DateTimeKind.Unspecified).AddTicks(630), new TimeSpan(0, 0, 0, 0, 0))),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTimeOffset(new DateTime(2025, 9, 29, 2, 35, 8, 342, DateTimeKind.Unspecified).AddTicks(880), new TimeSpan(0, 0, 0, 0, 0))),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    IngestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SourceRecords");
        }
    }
}
