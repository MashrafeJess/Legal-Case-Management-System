using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class SeventhCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalaryId",
                table: "User",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaseStatus",
                table: "Case",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Salary",
                columns: table => new
                {
                    SalaryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salary", x => x.SalaryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_SalaryId",
                table: "User",
                column: "SalaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Salary_SalaryId",
                table: "User",
                column: "SalaryId",
                principalTable: "Salary",
                principalColumn: "SalaryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Salary_SalaryId",
                table: "User");

            migrationBuilder.DropTable(
                name: "Salary");

            migrationBuilder.DropIndex(
                name: "IX_User_SalaryId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "SalaryId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CaseStatus",
                table: "Case");
        }
    }
}
