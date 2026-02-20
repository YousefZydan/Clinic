using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editDoctorsDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateIndex(
                name: "IX_WorkingTimes_DoctorDetailsId",
                table: "WorkingTimes",
                column: "DoctorDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDetails_DoctorId",
                table: "DoctorDetails",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorDetails_Doctors_DoctorId",
                table: "DoctorDetails",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkingTimes_DoctorDetails_DoctorDetailsId",
                table: "WorkingTimes",
                column: "DoctorDetailsId",
                principalTable: "DoctorDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateIndex(
                name: "IX_WorkingTimes_DoctorDetailsId",
                table: "WorkingTimes",
                column: "DoctorDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDetails_DoctorId",
                table: "DoctorDetails",
                column: "DoctorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorDetails_Doctors_DoctorId",
                table: "DoctorDetails",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkingTimes_DoctorDetails_DoctorDetailsId",
                table: "WorkingTimes",
                column: "DoctorDetailsId",
                principalTable: "DoctorDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
