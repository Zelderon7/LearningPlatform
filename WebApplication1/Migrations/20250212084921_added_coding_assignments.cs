using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class added_coding_assignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "CodingTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "CodingTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "CodingTasks",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxPoints",
                table: "CodingTasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodingTasks_ClassId",
                table: "CodingTasks",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_CodingTasks_Classes_ClassId",
                table: "CodingTasks",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodingTasks_Classes_ClassId",
                table: "CodingTasks");

            migrationBuilder.DropIndex(
                name: "IX_CodingTasks_ClassId",
                table: "CodingTasks");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "CodingTasks");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "CodingTasks");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "CodingTasks");

            migrationBuilder.DropColumn(
                name: "MaxPoints",
                table: "CodingTasks");
        }
    }
}
