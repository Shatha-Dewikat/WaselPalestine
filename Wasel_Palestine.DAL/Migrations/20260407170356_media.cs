using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel_Palestine.DAL.Migrations
{
    /// <inheritdoc />
    public partial class media : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoteType",
                table: "ReportMedias");

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "ReportMedias",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaUrl",
                table: "ReportMedias",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "ReportMedias");

            migrationBuilder.DropColumn(
                name: "MediaUrl",
                table: "ReportMedias");

            migrationBuilder.AddColumn<string>(
                name: "VoteType",
                table: "ReportMedias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
