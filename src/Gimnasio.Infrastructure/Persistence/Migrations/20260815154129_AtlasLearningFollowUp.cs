using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AtlasLearningFollowUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FollowUpObservation",
                table: "LearningEntries",
                type: "nvarchar(1600)",
                maxLength: 1600,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowUpOutcome",
                table: "LearningEntries",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReviewedOn",
                table: "LearningEntries",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowUpObservation",
                table: "LearningEntries");

            migrationBuilder.DropColumn(
                name: "FollowUpOutcome",
                table: "LearningEntries");

            migrationBuilder.DropColumn(
                name: "ReviewedOn",
                table: "LearningEntries");
        }
    }
}
