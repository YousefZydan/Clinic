using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_fav : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favourites_Patients_PatientId",
                table: "Favourites");

            migrationBuilder.DropIndex(
                name: "IX_Favourites_PatientId",
                table: "Favourites");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Favourites");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Favourites",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favourites_UserId",
                table: "Favourites",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favourites_AspNetUsers_UserId",
                table: "Favourites",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favourites_AspNetUsers_UserId",
                table: "Favourites");

            migrationBuilder.DropIndex(
                name: "IX_Favourites_UserId",
                table: "Favourites");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Favourites");

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "Favourites",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favourites_PatientId",
                table: "Favourites",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favourites_Patients_PatientId",
                table: "Favourites",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }
    }
}
