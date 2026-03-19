using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel_Palestine.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckpointStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CheckpointStatusId",
                table: "Checkpoints",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CheckpointStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckpointStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Checkpoints_CheckpointStatusId",
                table: "Checkpoints",
                column: "CheckpointStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Checkpoints_CheckpointStatuses_CheckpointStatusId",
                table: "Checkpoints",
                column: "CheckpointStatusId",
                principalTable: "CheckpointStatuses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Checkpoints_CheckpointStatuses_CheckpointStatusId",
                table: "Checkpoints");

            migrationBuilder.DropTable(
                name: "CheckpointStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Checkpoints_CheckpointStatusId",
                table: "Checkpoints");

            migrationBuilder.DropColumn(
                name: "CheckpointStatusId",
                table: "Checkpoints");
        }
    }
}
