using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelePsy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPsychologistPaymentAccountSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Psychologists");

            migrationBuilder.RenameColumn(
                name: "BankAccountType",
                table: "Psychologists",
                newName: "PaymentAccount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentAccount",
                table: "Psychologists",
                newName: "BankAccountType");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Psychologists",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
