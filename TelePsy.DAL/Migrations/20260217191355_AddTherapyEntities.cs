using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelePsy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTherapyEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                table: "Appointments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TherapyId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Therapies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Therapies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PsychologistTherapies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PsychologistId = table.Column<int>(type: "int", nullable: false),
                    TherapyId = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsychologistTherapies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PsychologistTherapies_Psychologists_PsychologistId",
                        column: x => x.PsychologistId,
                        principalTable: "Psychologists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PsychologistTherapies_Therapies_TherapyId",
                        column: x => x.TherapyId,
                        principalTable: "Therapies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TherapyId",
                table: "Appointments",
                column: "TherapyId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychologistTherapies_PsychologistId",
                table: "PsychologistTherapies",
                column: "PsychologistId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychologistTherapies_TherapyId",
                table: "PsychologistTherapies",
                column: "TherapyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Therapies_TherapyId",
                table: "Appointments",
                column: "TherapyId",
                principalTable: "Therapies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Therapies_TherapyId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "PsychologistTherapies");

            migrationBuilder.DropTable(
                name: "Therapies");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_TherapyId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "TherapyId",
                table: "Appointments");
        }
    }
}
