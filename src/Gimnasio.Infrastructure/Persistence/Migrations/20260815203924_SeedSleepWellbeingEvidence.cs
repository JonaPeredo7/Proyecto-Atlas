using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSleepWellbeingEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EvidenceReferences",
                columns: new[] { "Id", "Topic", "Title", "PermanentId", "SourceUrl", "Level", "Status", "PublishedOn", "NextReviewOn", "Applicability", "Limitations", "CreatedAt", "UpdatedAt", "IsActive" },
                values: new object[,]
                {
                    { new Guid("21000000-0000-0000-0000-000000000201"), "Sueño", "Recommended Amount of Sleep for a Healthy Adult: A Joint Consensus Statement of the American Academy of Sleep Medicine and Sleep Research Society", "PMID:25979105", "https://pubmed.ncbi.nlm.nih.gov/25979105/", 5, "informative", new DateOnly(2015, 6, 15), new DateOnly(2027, 8, 15), "Aporta una referencia poblacional de salud para contextualizar la duración del sueño en adultos.", "La recomendación general de siete horas o más no es una frontera diaria de recuperación deportiva ni una autorización individual para entrenar.", new DateTimeOffset(2026, 8, 15, 0, 0, 0, TimeSpan.Zero), null, true },
                    { new Guid("21000000-0000-0000-0000-000000000202"), "Sueño", "Sleep and the athlete: narrative review and 2021 expert consensus recommendations", "PMID:33144349", "https://pubmed.ncbi.nlm.nih.gov/33144349/", 1, "informative", new DateOnly(2020, 11, 3), new DateOnly(2027, 8, 15), "Sustenta un enfoque individualizado que considere necesidad percibida, contexto y obstáculos específicos del deportista.", "Es una revisión narrativa con consenso experto y reconoce limitaciones metodológicas; no valida una regla universal para decidir una sesión individual.", new DateTimeOffset(2026, 8, 15, 0, 0, 0, TimeSpan.Zero), null, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("EvidenceReferences", "Id", new Guid("21000000-0000-0000-0000-000000000201"));
            migrationBuilder.DeleteData("EvidenceReferences", "Id", new Guid("21000000-0000-0000-0000-000000000202"));
        }
    }
}
