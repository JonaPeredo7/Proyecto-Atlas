using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AtlasKneeTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KneeChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AthleteProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Context = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Side = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PainNow = table.Column<int>(type: "int", nullable: false),
                    PainBest24H = table.Column<int>(type: "int", nullable: false),
                    PainWorst24H = table.Column<int>(type: "int", nullable: false),
                    Swelling = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Instability = table.Column<bool>(type: "bit", nullable: false),
                    Locking = table.Column<bool>(type: "bit", nullable: false),
                    FullExtension = table.Column<bool>(type: "bit", nullable: false),
                    WalkingCapacity = table.Column<int>(type: "int", nullable: false),
                    StairsCapacity = table.Column<int>(type: "int", nullable: false),
                    SquatCapacity = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KneeChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KneeChecks_AthleteProfiles_AthleteProfileId",
                        column: x => x.AthleteProfileId,
                        principalTable: "AthleteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KneeChecks_AthleteProfileId_RecordedAt",
                table: "KneeChecks",
                columns: new[] { "AthleteProfileId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KneeChecks");
        }
    }
}
