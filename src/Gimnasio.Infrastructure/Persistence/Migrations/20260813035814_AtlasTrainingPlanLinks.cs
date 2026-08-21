using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AtlasTrainingPlanLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PersonalGoalId",
                table: "TrainingSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TrainingCycleId",
                table: "TrainingSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "TrainingSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_PersonalGoalId",
                table: "TrainingSessions",
                column: "PersonalGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_TrainingCycleId",
                table: "TrainingSessions",
                column: "TrainingCycleId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSessions_PersonalGoals_PersonalGoalId",
                table: "TrainingSessions",
                column: "PersonalGoalId",
                principalTable: "PersonalGoals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSessions_TrainingCycles_TrainingCycleId",
                table: "TrainingSessions",
                column: "TrainingCycleId",
                principalTable: "TrainingCycles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSessions_PersonalGoals_PersonalGoalId",
                table: "TrainingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSessions_TrainingCycles_TrainingCycleId",
                table: "TrainingSessions");

            migrationBuilder.DropIndex(
                name: "IX_TrainingSessions_PersonalGoalId",
                table: "TrainingSessions");

            migrationBuilder.DropIndex(
                name: "IX_TrainingSessions_TrainingCycleId",
                table: "TrainingSessions");

            migrationBuilder.DropColumn(
                name: "PersonalGoalId",
                table: "TrainingSessions");

            migrationBuilder.DropColumn(
                name: "TrainingCycleId",
                table: "TrainingSessions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "TrainingSessions");
        }
    }
}
