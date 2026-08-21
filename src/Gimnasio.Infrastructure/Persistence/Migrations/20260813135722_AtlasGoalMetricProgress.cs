using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AtlasGoalMetricProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MetricDefinitionId",
                table: "PersonalGoals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalGoals_MetricDefinitionId",
                table: "PersonalGoals",
                column: "MetricDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalGoals_MetricDefinitions_MetricDefinitionId",
                table: "PersonalGoals",
                column: "MetricDefinitionId",
                principalTable: "MetricDefinitions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalGoals_MetricDefinitions_MetricDefinitionId",
                table: "PersonalGoals");

            migrationBuilder.DropIndex(
                name: "IX_PersonalGoals_MetricDefinitionId",
                table: "PersonalGoals");

            migrationBuilder.DropColumn(
                name: "MetricDefinitionId",
                table: "PersonalGoals");
        }
    }
}
