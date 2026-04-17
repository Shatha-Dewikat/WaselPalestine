using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Wasel_Palestine.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FinalFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentHistories_AspNetUsers_ChangedByUserId1",
                table: "IncidentHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentCategories_CategoryId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentSeverities_SeverityId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentStatuses_StatusId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_IncidentHistories_ChangedByUserId1",
                table: "IncidentHistories");

            migrationBuilder.DropColumn(
                name: "VoteType",
                table: "ReportMedias");

            migrationBuilder.DropColumn(
                name: "ChangedByUserId1",
                table: "IncidentHistories");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "CheckpointStatusHistories",
                newName: "OldStatus");

          
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Locations",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Locations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AreaName",
                table: "Locations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Point>(
                name: "Coordinates",
                table: "Locations",
                type: "geography",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "IncidentStatuses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "IncidentSeverities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Incidents",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<int>(
                name: "RelatedCheckpointId",
                table: "Incidents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "IncidentMedias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "IncidentMedias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "IncidentMedias",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "ChangedByUserId",
                table: "IncidentHistories",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "IncidentHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Changes",
                table: "IncidentHistories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "IncidentCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "IncidentCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewStatus",
                table: "CheckpointStatusHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CheckpointStatusId",
                table: "Checkpoints",
                type: "int",
                nullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AuditLogs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "AuditLogs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "AuditLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "IPAddress",
                table: "AuditLogs",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "AuditLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "AuditLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CodeResetPassword",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetCodeExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CheckpointStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckpointStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CityIncidentStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActiveIncidentsCount = table.Column<int>(type: "int", nullable: false),
                    ClosedCheckpointsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityIncidentStats", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_RelatedCheckpointId",
                table: "Incidents",
                column: "RelatedCheckpointId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentHistories_ChangedByUserId",
                table: "IncidentHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Checkpoints_CheckpointStatusId",
                table: "Checkpoints",
                column: "CheckpointStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Checkpoints_CheckpointStatuses_CheckpointStatusId",
                table: "Checkpoints",
                column: "CheckpointStatusId",
                principalTable: "CheckpointStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentHistories_AspNetUsers_ChangedByUserId",
                table: "IncidentHistories",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Checkpoints_RelatedCheckpointId",
                table: "Incidents",
                column: "RelatedCheckpointId",
                principalTable: "Checkpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

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
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Checkpoints_CheckpointStatuses_CheckpointStatusId",
                table: "Checkpoints");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentHistories_AspNetUsers_ChangedByUserId",
                table: "IncidentHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Checkpoints_RelatedCheckpointId",
                table: "Incidents");

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

            migrationBuilder.DropTable(
                name: "CheckpointStatuses");

            migrationBuilder.DropTable(
                name: "CityIncidentStats");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_IncidentCategoryId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_IncidentSeverityId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_IncidentStatusId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_RelatedCheckpointId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_IncidentHistories_ChangedByUserId",
                table: "IncidentHistories");

            migrationBuilder.DropIndex(
                name: "IX_Checkpoints_CheckpointStatusId",
                table: "Checkpoints");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "ReportMedias");

            migrationBuilder.DropColumn(
                name: "MediaUrl",
                table: "ReportMedias");

            migrationBuilder.DropColumn(
                name: "Coordinates",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "IncidentStatuses");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "IncidentSeverities");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
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

            migrationBuilder.DropColumn(
                name: "RelatedCheckpointId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "IncidentMedias");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "IncidentMedias");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "IncidentMedias");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "IncidentHistories");

            migrationBuilder.DropColumn(
                name: "Changes",
                table: "IncidentHistories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "IncidentCategories");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "IncidentCategories");

            migrationBuilder.DropColumn(
                name: "NewStatus",
                table: "CheckpointStatusHistories");

            migrationBuilder.DropColumn(
                name: "CheckpointStatusId",
                table: "Checkpoints");

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

            migrationBuilder.DropColumn(
                name: "CodeResetPassword",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeExpiry",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Alerts");

            migrationBuilder.RenameColumn(
                name: "OldStatus",
                table: "CheckpointStatusHistories",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Checkpoints",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoteType",
                table: "ReportMedias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Locations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "AreaName",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Incidents",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AuditLogs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "AuditLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "IPAddress",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45);

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentHistories_ChangedByUserId1",
                table: "IncidentHistories",
                column: "ChangedByUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentHistories_AspNetUsers_ChangedByUserId1",
                table: "IncidentHistories",
                column: "ChangedByUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

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
