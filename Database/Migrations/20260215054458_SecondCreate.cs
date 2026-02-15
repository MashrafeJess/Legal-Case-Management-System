using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class SecondCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Case_CaseType_TypeCaseTypeId",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "CaseType",
                table: "Case");

            migrationBuilder.RenameColumn(
                name: "TypeCaseTypeId",
                table: "Case",
                newName: "CaseTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Case_TypeCaseTypeId",
                table: "Case",
                newName: "IX_Case_CaseTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Case_CaseType_CaseTypeId",
                table: "Case",
                column: "CaseTypeId",
                principalTable: "CaseType",
                principalColumn: "CaseTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Case_CaseType_CaseTypeId",
                table: "Case");

            migrationBuilder.RenameColumn(
                name: "CaseTypeId",
                table: "Case",
                newName: "TypeCaseTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Case_CaseTypeId",
                table: "Case",
                newName: "IX_Case_TypeCaseTypeId");

            migrationBuilder.AddColumn<int>(
                name: "CaseType",
                table: "Case",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Case_CaseType_TypeCaseTypeId",
                table: "Case",
                column: "TypeCaseTypeId",
                principalTable: "CaseType",
                principalColumn: "CaseTypeId");
        }
    }
}
