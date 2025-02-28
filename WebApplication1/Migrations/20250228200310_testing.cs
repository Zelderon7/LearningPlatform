using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class testing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "CodingTasks");

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "CodingTaskSubmissions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxPoints",
                table: "CodingTasks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "CodingTasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CodingFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodingFolders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodingFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FolderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodingFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodingFiles_CodingFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "CodingFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FolderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTemplates_CodingFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "CodingFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    FolderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTasks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTasks_CodingFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "CodingFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTasks_CodingTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "CodingTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodingTaskSubmissions_FolderId",
                table: "CodingTaskSubmissions",
                column: "FolderId",
                unique: true,
                filter: "[FolderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CodingTasks_FolderId",
                table: "CodingTasks",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_CodingFiles_FolderId",
                table: "CodingFiles",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplates_FolderId",
                table: "TaskTemplates",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_FolderId",
                table: "UserTasks",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_TaskId",
                table: "UserTasks",
                column: "TaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_UserId",
                table: "UserTasks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CodingTasks_CodingFolders_FolderId",
                table: "CodingTasks",
                column: "FolderId",
                principalTable: "CodingFolders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CodingTaskSubmissions_CodingFolders_FolderId",
                table: "CodingTaskSubmissions",
                column: "FolderId",
                principalTable: "CodingFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodingTasks_CodingFolders_FolderId",
                table: "CodingTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_CodingTaskSubmissions_CodingFolders_FolderId",
                table: "CodingTaskSubmissions");

            migrationBuilder.DropTable(
                name: "CodingFiles");

            migrationBuilder.DropTable(
                name: "TaskTemplates");

            migrationBuilder.DropTable(
                name: "UserTasks");

            migrationBuilder.DropTable(
                name: "CodingFolders");

            migrationBuilder.DropIndex(
                name: "IX_CodingTaskSubmissions_FolderId",
                table: "CodingTaskSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_CodingTasks_FolderId",
                table: "CodingTasks");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "CodingTaskSubmissions");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "CodingTasks");

            migrationBuilder.AlterColumn<int>(
                name: "MaxPoints",
                table: "CodingTasks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "CodingTasks",
                type: "datetime2",
                nullable: true);
        }
    }
}
