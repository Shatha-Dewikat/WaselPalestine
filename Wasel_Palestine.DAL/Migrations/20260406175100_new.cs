using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel_Palestine.DAL.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Alerts");
        }
    }
}
