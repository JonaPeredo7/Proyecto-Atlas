using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AtlasWorkContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreakMinutes",
                table: "DailyActivities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnusualConditions",
                table: "DailyActivities",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkDemands",
                table: "DailyActivities",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakMinutes",
                table: "DailyActivities");

            migrationBuilder.DropColumn(
                name: "UnusualConditions",
                table: "DailyActivities");

            migrationBuilder.DropColumn(
                name: "WorkDemands",
                table: "DailyActivities");
        }
    }
}
