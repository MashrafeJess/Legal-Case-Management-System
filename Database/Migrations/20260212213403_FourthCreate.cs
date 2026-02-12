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
            migrationBuilder.DropForeignKey(
                name: "FK_FileEntity_User_CreatedByUserUserId",
                table: "FileEntity");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserUserId",
                table: "FileEntity",
                newName: "CaseUserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_FileEntity_CreatedByUserUserId",
                table: "FileEntity",
                newName: "IX_FileEntity_CaseUserUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Case",
                newName: "Email");

            migrationBuilder.AddColumn<string>(
                name: "CaseTypeName",
                table: "CaseType",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TypeCaseTypeId",
                table: "Case",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileEntity_CaseId",
                table: "FileEntity",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Case_CaseHandlingBy",
                table: "Case",
                column: "CaseHandlingBy");

            migrationBuilder.CreateIndex(
                name: "IX_Case_TypeCaseTypeId",
                table: "Case",
                column: "TypeCaseTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Case_CaseType_TypeCaseTypeId",
                table: "Case",
                column: "TypeCaseTypeId",
                principalTable: "CaseType",
                principalColumn: "CaseTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Case_User_CaseHandlingBy",
                table: "Case",
                column: "CaseHandlingBy",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntity_Case_CaseId",
                table: "FileEntity",
                column: "CaseId",
                principalTable: "Case",
                principalColumn: "CaseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntity_User_CaseUserUserId",
                table: "FileEntity",
                column: "CaseUserUserId",
                principalTable: "User",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Case_CaseType_TypeCaseTypeId",
                table: "Case");

            migrationBuilder.DropForeignKey(
                name: "FK_Case_User_CaseHandlingBy",
                table: "Case");

            migrationBuilder.DropForeignKey(
                name: "FK_FileEntity_Case_CaseId",
                table: "FileEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_FileEntity_User_CaseUserUserId",
                table: "FileEntity");

            migrationBuilder.DropIndex(
                name: "IX_FileEntity_CaseId",
                table: "FileEntity");

            migrationBuilder.DropIndex(
                name: "IX_Case_CaseHandlingBy",
                table: "Case");

            migrationBuilder.DropIndex(
                name: "IX_Case_TypeCaseTypeId",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "CaseTypeName",
                table: "CaseType");

            migrationBuilder.DropColumn(
                name: "TypeCaseTypeId",
                table: "Case");

            migrationBuilder.RenameColumn(
                name: "CaseUserUserId",
                table: "FileEntity",
                newName: "CreatedByUserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_FileEntity_CaseUserUserId",
                table: "FileEntity",
                newName: "IX_FileEntity_CreatedByUserUserId");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Case",
                newName: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntity_User_CreatedByUserUserId",
                table: "FileEntity",
                column: "CreatedByUserUserId",
                principalTable: "User",
                principalColumn: "UserId");
        }
    }
}
