using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityServerSystem.Data.Migrations
{
    public partial class RemoveDepartmentFromUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ApplicationDepartments_DepartmentID",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ApplicationDepartments");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DepartmentID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DepartmentID",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentID",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationDepartments",
                columns: table => new
                {
                    DepartmentID = table.Column<Guid>(nullable: false),
                    DepartmentName = table.Column<string>(nullable: false),
                    Remarks = table.Column<string>(nullable: true),
                    TimesStamp = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDepartments", x => x.DepartmentID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DepartmentID",
                table: "AspNetUsers",
                column: "DepartmentID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ApplicationDepartments_DepartmentID",
                table: "AspNetUsers",
                column: "DepartmentID",
                principalTable: "ApplicationDepartments",
                principalColumn: "DepartmentID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
