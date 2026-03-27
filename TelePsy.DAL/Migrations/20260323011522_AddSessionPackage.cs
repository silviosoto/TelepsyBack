using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelePsy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SessionPackages_PaymentId",
                table: "SessionPackages");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentId",
                table: "SessionPackages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPackages_PaymentId",
                table: "SessionPackages",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SessionPackages_PaymentId",
                table: "SessionPackages");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentId",
                table: "SessionPackages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionPackages_PaymentId",
                table: "SessionPackages",
                column: "PaymentId",
                unique: true);
        }
    }
}
