using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel_Palestine.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CheckPints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Checkpoints",
                newName: "NameEn");

            migrationBuilder.AddColumn<double>(
                name: "ConfidenceScore",
                table: "Checkpoints",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Checkpoints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Checkpoints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDelayMinutes",
                table: "Checkpoints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Checkpoints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "Checkpoints");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Checkpoints");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Checkpoints");

            migrationBuilder.DropColumn(
                name: "EstimatedDelayMinutes",
                table: "Checkpoints");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Checkpoints");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Checkpoints",
                newName: "Name");
        }
    }
}
