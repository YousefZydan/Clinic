using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editingWorkingTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "WorkingTimes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkingTimes_PatientId",
                table: "WorkingTimes",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkingTimes_Patients_PatientId",
                table: "WorkingTimes",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkingTimes_Patients_PatientId",
                table: "WorkingTimes");

            migrationBuilder.DropIndex(
                name: "IX_WorkingTimes_PatientId",
                table: "WorkingTimes");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "WorkingTimes");
        }
    }
}
