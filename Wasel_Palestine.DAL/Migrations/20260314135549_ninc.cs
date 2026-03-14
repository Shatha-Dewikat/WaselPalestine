using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel_Palestine.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ninc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentCategories_CategoryId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentSeverities_SeverityId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentStatuses_StatusId",
                table: "Incidents");

            migrationBuilder.AddColumn<int>(
                name: "IncidentCategoryId",
                table: "Incidents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IncidentSeverityId",
                table: "Incidents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IncidentStatusId",
                table: "Incidents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_IncidentCategoryId",
                table: "Incidents",
                column: "IncidentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_IncidentSeverityId",
                table: "Incidents",
                column: "IncidentSeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_IncidentStatusId",
                table: "Incidents",
                column: "IncidentStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentCategories_CategoryId",
                table: "Incidents",
                column: "CategoryId",
                principalTable: "IncidentCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentCategories_IncidentCategoryId",
                table: "Incidents",
                column: "IncidentCategoryId",
                principalTable: "IncidentCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentSeverities_IncidentSeverityId",
                table: "Incidents",
                column: "IncidentSeverityId",
                principalTable: "IncidentSeverities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentSeverities_SeverityId",
                table: "Incidents",
                column: "SeverityId",
                principalTable: "IncidentSeverities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentStatuses_IncidentStatusId",
                table: "Incidents",
                column: "IncidentStatusId",
                principalTable: "IncidentStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentStatuses_StatusId",
                table: "Incidents",
                column: "StatusId",
                principalTable: "IncidentStatuses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentCategories_CategoryId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentCategories_IncidentCategoryId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentSeverities_IncidentSeverityId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentSeverities_SeverityId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentStatuses_IncidentStatusId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentStatuses_StatusId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_IncidentCategoryId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_IncidentSeverityId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_IncidentStatusId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "IncidentCategoryId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "IncidentSeverityId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "IncidentStatusId",
                table: "Incidents");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentCategories_CategoryId",
                table: "Incidents",
                column: "CategoryId",
                principalTable: "IncidentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentSeverities_SeverityId",
                table: "Incidents",
                column: "SeverityId",
                principalTable: "IncidentSeverities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentStatuses_StatusId",
                table: "Incidents",
                column: "StatusId",
                principalTable: "IncidentStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
