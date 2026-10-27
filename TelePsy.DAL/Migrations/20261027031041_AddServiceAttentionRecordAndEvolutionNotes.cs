using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelePsy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceAttentionRecordAndEvolutionNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FocusedMentalExam",
                table: "PsychologyNotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ServiceAttentionRecordId",
                table: "PsychologyNotes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceAttentionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PsychologistId = table.Column<int>(type: "int", nullable: false),
                    TherapyId = table.Column<int>(type: "int", nullable: false),
                    InitialAppointmentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvaluationData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TreatmentPlan = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAttentionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceAttentionRecords_Appointments_InitialAppointmentId",
                        column: x => x.InitialAppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceAttentionRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceAttentionRecords_Psychologists_PsychologistId",
                        column: x => x.PsychologistId,
                        principalTable: "Psychologists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceAttentionRecords_Therapies_TherapyId",
                        column: x => x.TherapyId,
                        principalTable: "Therapies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PsychologyNotes_ServiceAttentionRecordId",
                table: "PsychologyNotes",
                column: "ServiceAttentionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAttentionRecords_InitialAppointmentId",
                table: "ServiceAttentionRecords",
                column: "InitialAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAttentionRecords_PatientId",
                table: "ServiceAttentionRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAttentionRecords_PsychologistId",
                table: "ServiceAttentionRecords",
                column: "PsychologistId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAttentionRecords_TherapyId",
                table: "ServiceAttentionRecords",
                column: "TherapyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PsychologyNotes_ServiceAttentionRecords_ServiceAttentionRecordId",
                table: "PsychologyNotes",
                column: "ServiceAttentionRecordId",
                principalTable: "ServiceAttentionRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PsychologyNotes_ServiceAttentionRecords_ServiceAttentionRecordId",
                table: "PsychologyNotes");

            migrationBuilder.DropTable(
                name: "ServiceAttentionRecords");

            migrationBuilder.DropIndex(
                name: "IX_PsychologyNotes_ServiceAttentionRecordId",
                table: "PsychologyNotes");

            migrationBuilder.DropColumn(
                name: "FocusedMentalExam",
                table: "PsychologyNotes");

            migrationBuilder.DropColumn(
                name: "ServiceAttentionRecordId",
                table: "PsychologyNotes");
        }
    }
}
