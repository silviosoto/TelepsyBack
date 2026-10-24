using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelePsy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddReasonForConsultationToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReasonForConsultation",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReasonForConsultation",
                table: "Appointments");
        }
    }
}
