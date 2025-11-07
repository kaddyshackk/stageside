using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComedyPull.Data.Migrations.PipelineDb
{
    /// <inheritdoc />
    public partial class PipelineDbAddRegexFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SitemapUrl",
                table: "JobSitemaps",
                newName: "Url");

            migrationBuilder.AddColumn<string>(
                name: "RegexFilter",
                table: "JobSitemaps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegexFilter",
                table: "JobSitemaps");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "JobSitemaps",
                newName: "SitemapUrl");
        }
    }
}
