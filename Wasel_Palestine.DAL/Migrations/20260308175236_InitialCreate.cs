using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel_Palestine.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "Incidents",
                type: "nvarchar(max)",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "IncidentCategories");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "IncidentCategories");
        }
    }
}
