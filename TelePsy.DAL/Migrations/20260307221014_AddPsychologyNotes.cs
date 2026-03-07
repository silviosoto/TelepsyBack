using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelePsy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPsychologyNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PsychologyNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PsychologistId = table.Column<int>(type: "int", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionNumber = table.Column<int>(type: "int", nullable: false),
                    ReasonForSession = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Evolution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Interventions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TherapeuticPlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextAppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProfessionalSignature = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsychologyNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PsychologyNotes_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PsychologyNotes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PsychologyNotes_Psychologists_PsychologistId",
                        column: x => x.PsychologistId,
                        principalTable: "Psychologists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PsychologyNotes_AppointmentId",
                table: "PsychologyNotes",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychologyNotes_PatientId",
                table: "PsychologyNotes",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychologyNotes_PsychologistId",
                table: "PsychologyNotes",
                column: "PsychologistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PsychologyNotes");
        }
    }
}
