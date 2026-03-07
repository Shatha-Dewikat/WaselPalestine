using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel_Palestine.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CorrectIncidentHistoryFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentHistories_AspNetUsers_ChangedByUserId1",
                table: "IncidentHistories");

            migrationBuilder.DropIndex(
                name: "IX_IncidentHistories_ChangedByUserId1",
                table: "IncidentHistories");

            migrationBuilder.DropColumn(
                name: "ChangedByUserId1",
                table: "IncidentHistories");

            migrationBuilder.AlterColumn<string>(
                name: "ChangedByUserId",
                table: "IncidentHistories",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentHistories_ChangedByUserId",
                table: "IncidentHistories",
                column: "ChangedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentHistories_AspNetUsers_ChangedByUserId",
                table: "IncidentHistories",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentHistories_AspNetUsers_ChangedByUserId",
                table: "IncidentHistories");

            migrationBuilder.DropIndex(
                name: "IX_IncidentHistories_ChangedByUserId",
                table: "IncidentHistories");

            migrationBuilder.AlterColumn<int>(
                name: "ChangedByUserId",
                table: "IncidentHistories",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ChangedByUserId1",
                table: "IncidentHistories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentHistories_ChangedByUserId1",
                table: "IncidentHistories",
                column: "ChangedByUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentHistories_AspNetUsers_ChangedByUserId1",
                table: "IncidentHistories",
                column: "ChangedByUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
