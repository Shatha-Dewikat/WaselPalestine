using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel_Palestine.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Linked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelatedCheckpointId",
                table: "Incidents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_RelatedCheckpointId",
                table: "Incidents",
                column: "RelatedCheckpointId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Checkpoints_RelatedCheckpointId",
                table: "Incidents",
                column: "RelatedCheckpointId",
                principalTable: "Checkpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Checkpoints_RelatedCheckpointId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_RelatedCheckpointId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "RelatedCheckpointId",
                table: "Incidents");
        }
    }
}
