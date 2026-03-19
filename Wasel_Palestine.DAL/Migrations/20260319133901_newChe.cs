using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel_Palestine.DAL.Migrations
{
    /// <inheritdoc />
    public partial class newChe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "CheckpointStatusHistories",
                newName: "OldStatus");

            migrationBuilder.AddColumn<string>(
                name: "NewStatus",
                table: "CheckpointStatusHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewStatus",
                table: "CheckpointStatusHistories");

            migrationBuilder.RenameColumn(
                name: "OldStatus",
                table: "CheckpointStatusHistories",
                newName: "Status");
        }
    }
}
