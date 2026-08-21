using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AtlasActivityPlanSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlannedDurationMinutes",
                table: "DailyActivities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlannedSource",
                table: "DailyActivities",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedDurationMinutes",
                table: "DailyActivities");

            migrationBuilder.DropColumn(
                name: "PlannedSource",
                table: "DailyActivities");
        }
    }
}
