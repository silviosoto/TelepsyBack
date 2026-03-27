using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelePsy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PatientJoinedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PsychologistJoinedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionPackageId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SessionPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PsychologistId = table.Column<int>(type: "int", nullable: false),
                    TherapyId = table.Column<int>(type: "int", nullable: false),
                    OriginalTotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    UsedSessions = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionPackages_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionPackages_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionPackages_Psychologists_PsychologistId",
                        column: x => x.PsychologistId,
                        principalTable: "Psychologists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionPackages_Therapies_TherapyId",
                        column: x => x.TherapyId,
                        principalTable: "Therapies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SessionPackageId",
                table: "Appointments",
                column: "SessionPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPackages_PatientId",
                table: "SessionPackages",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPackages_PaymentId",
                table: "SessionPackages",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionPackages_PsychologistId",
                table: "SessionPackages",
                column: "PsychologistId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPackages_TherapyId",
                table: "SessionPackages",
                column: "TherapyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_SessionPackages_SessionPackageId",
                table: "Appointments",
                column: "SessionPackageId",
                principalTable: "SessionPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_SessionPackages_SessionPackageId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "SessionPackages");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_SessionPackageId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PatientJoinedAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PsychologistJoinedAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "SessionPackageId",
                table: "Appointments");
        }
    }
}
