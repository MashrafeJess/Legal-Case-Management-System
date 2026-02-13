using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class SixthCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileEntity_User_CaseUserUserId",
                table: "FileEntity");

            migrationBuilder.DropIndex(
                name: "IX_FileEntity_CaseUserUserId",
                table: "FileEntity");

            migrationBuilder.DropColumn(
                name: "CaseUserUserId",
                table: "FileEntity");

            migrationBuilder.AddColumn<int>(
                name: "CaseId",
                table: "Payment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Payment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Payment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationId",
                table: "Payment",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                table: "PaymentMethod",
                columns: new[] { "PaymentMethodId", "CreatedBy", "CreatedDate", "IsDeleted", "PaymentMethodName", "PaymentStatus", "UpdatedBy", "UpdatedDate" },
                values: new object[] { 1, null, new DateTime(2024, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc), false, "Bkash", false, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentMethodId",
                table: "Payment",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_PaymentMethod_PaymentMethodId",
                table: "Payment",
                column: "PaymentMethodId",
                principalTable: "PaymentMethod",
                principalColumn: "PaymentMethodId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_PaymentMethod_PaymentMethodId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_PaymentMethodId",
                table: "Payment");

            migrationBuilder.DeleteData(
                table: "PaymentMethod",
                keyColumn: "PaymentMethodId",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "CaseId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "ValidationId",
                table: "Payment");

            migrationBuilder.AddColumn<string>(
                name: "CaseUserUserId",
                table: "FileEntity",
                type: "character varying(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileEntity_CaseUserUserId",
                table: "FileEntity",
                column: "CaseUserUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntity_User_CaseUserUserId",
                table: "FileEntity",
                column: "CaseUserUserId",
                principalTable: "User",
                principalColumn: "UserId");
        }
    }
}
