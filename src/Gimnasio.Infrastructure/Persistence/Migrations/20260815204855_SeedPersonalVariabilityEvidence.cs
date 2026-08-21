using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPersonalVariabilityEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EvidenceReferences",
                columns: new[] { "Id", "Topic", "Title", "PermanentId", "SourceUrl", "Level", "Status", "PublishedOn", "NextReviewOn", "Applicability", "Limitations", "CreatedAt", "UpdatedAt", "IsActive" },
                values: new object[,]
                {
                    { new Guid("21000000-0000-0000-0000-000000000203"), "Bienestar", "Monitoring the athlete training response: subjective self-reported measures trump commonly used objective measures: a systematic review", "PMID:26423706", "https://pubmed.ncbi.nlm.nih.gov/26423706/", 4, "informative", new DateOnly(2015, 9, 30), new DateOnly(2027, 8, 15), "Sustenta el seguimiento subjetivo repetido y la construcción de una referencia individual para describir cambios.", "Los autorreportes y las medidas objetivas no son intercambiables; la revisión no valida los cortes visuales propios de Atlas ni autoriza decisiones clínicas.", new DateTimeOffset(2026, 8, 15, 0, 0, 0, TimeSpan.Zero), null, true },
                    { new Guid("21000000-0000-0000-0000-000000000204"), "Sueño", "Intraindividual variability in sleep among athletes: A systematic review of definitions, operationalizations, and key correlates", "PMID:37485972", "https://pubmed.ncbi.nlm.nih.gov/37485972/", 4, "informative", new DateOnly(2023, 7, 24), new DateOnly(2027, 8, 15), "Sustenta conservar y mostrar la variación del sueño dentro de una misma persona a lo largo del tiempo.", "La literatura utiliza definiciones y formas de cálculo heterogéneas; el rango descriptivo de Atlas no debe interpretarse como normalidad clínica ni objetivo prescriptivo.", new DateTimeOffset(2026, 8, 15, 0, 0, 0, TimeSpan.Zero), null, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("EvidenceReferences", "Id", new Guid("21000000-0000-0000-0000-000000000203"));
            migrationBuilder.DeleteData("EvidenceReferences", "Id", new Guid("21000000-0000-0000-0000-000000000204"));
        }
    }
}
