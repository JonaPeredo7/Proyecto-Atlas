using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AtlasLearningJournal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AthleteProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Observation = table.Column<string>(type: "nvarchar(1600)", maxLength: 1600, nullable: false),
                    Interpretation = table.Column<string>(type: "nvarchar(1600)", maxLength: 1600, nullable: true),
                    NextAction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Confidence = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PersonalGoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrainingCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningEntries_AthleteProfiles_AthleteProfileId",
                        column: x => x.AthleteProfileId,
                        principalTable: "AthleteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningEntries_PersonalGoals_PersonalGoalId",
                        column: x => x.PersonalGoalId,
                        principalTable: "PersonalGoals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LearningEntries_TrainingCycles_TrainingCycleId",
                        column: x => x.TrainingCycleId,
                        principalTable: "TrainingCycles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LearningEntries_TrainingSessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalTable: "TrainingSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningEntries_AthleteProfileId_Date",
                table: "LearningEntries",
                columns: new[] { "AthleteProfileId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_LearningEntries_PersonalGoalId",
                table: "LearningEntries",
                column: "PersonalGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningEntries_TrainingCycleId",
                table: "LearningEntries",
                column: "TrainingCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningEntries_TrainingSessionId",
                table: "LearningEntries",
                column: "TrainingSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningEntries");
        }
    }
}
