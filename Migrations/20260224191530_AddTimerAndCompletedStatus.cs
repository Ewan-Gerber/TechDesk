using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechDesk.Migrations
{
    /// <inheritdoc />
    public partial class AddTimerAndCompletedStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimerEndDate",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimerStartDate",
                table: "Tickets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimerEndDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TimerStartDate",
                table: "Tickets");
        }
    }
}
