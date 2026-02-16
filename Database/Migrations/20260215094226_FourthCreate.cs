using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class FourthCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HearingId",
                table: "Payment",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hearing_CaseId",
                table: "Hearing",
                column: "CaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hearing_Case_CaseId",
                table: "Hearing",
                column: "CaseId",
                principalTable: "Case",
                principalColumn: "CaseId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hearing_Case_CaseId",
                table: "Hearing");

            migrationBuilder.DropIndex(
                name: "IX_Hearing_CaseId",
                table: "Hearing");

            migrationBuilder.DropColumn(
                name: "HearingId",
                table: "Payment");
        }
    }
}
