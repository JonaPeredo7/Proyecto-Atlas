using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedWorkRecoveryEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EvidenceReferences",
                columns: new[] { "Id", "Topic", "Title", "PermanentId", "SourceUrl", "Level", "Status", "PublishedOn", "NextReviewOn", "Applicability", "Limitations", "CreatedAt", "UpdatedAt", "IsActive" },
                values: new object[,]
                {
                    { new Guid("21000000-0000-0000-0000-000000000101"), "Carga laboral", "Global, regional and national burdens of ischaemic heart disease and stroke attributable to exposure to long working hours", "WHO/ILO:2021-long-working-hours", "https://www.who.int/news-room/questions-and-answers/item/global-regional-and-national-burdens-of-ischemic-heart-disease-and-stroke-attributable-to-exposure-to-long-working-hours-for-194-countries-2000-2016", 4, "informative", new DateOnly(2021, 5, 17), new DateOnly(2027, 8, 15), "Sustenta conservar y revisar la duración semanal real del trabajo como una exposición diferenciada.", "El umbral poblacional de 55 horas semanales se refiere a resultados cardiovasculares de largo plazo; no estima recuperación diaria ni indica cómo ajustar un entrenamiento individual.", new DateTimeOffset(2026, 8, 15, 0, 0, 0, TimeSpan.Zero), null, true },
                    { new Guid("21000000-0000-0000-0000-000000000102"), "Carga laboral", "Leisure-time physical activity, occupational physical activity and the physical activity paradox in healthcare workers: A systematic overview of the literature", "PMID:36966711", "https://pubmed.ncbi.nlm.nih.gov/36966711/", 4, "informative", new DateOnly(2023, 2, 18), new DateOnly(2027, 8, 15), "Sustenta registrar la actividad física laboral y el entrenamiento como dominios separados con duración e intensidad propias.", "La certeza fue baja, el riesgo de sesgo moderado a alto y las tareas de trabajadores sanitarios no equivalen necesariamente al trabajo concreto del usuario.", new DateTimeOffset(2026, 8, 15, 0, 0, 0, TimeSpan.Zero), null, true },
                    { new Guid("21000000-0000-0000-0000-000000000103"), "Carga laboral", "Associations of occupational and leisure-time physical activity with all-cause mortality: an individual participant data meta-analysis", "PMID:39255999", "https://pubmed.ncbi.nlm.nih.gov/39255999/", 4, "informative", new DateOnly(2024, 9, 10), new DateOnly(2027, 8, 15), "Sustenta no asumir que la actividad física laboral produce los mismos efectos que el ejercicio realizado durante el ocio.", "Metaanálisis observacional sobre mortalidad a largo plazo; no mide la respuesta inmediata, la carga articular ni el rendimiento individual. Se tuvo en cuenta la corrección editorial publicada en 2025.", new DateTimeOffset(2026, 8, 15, 0, 0, 0, TimeSpan.Zero), null, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var id in new[]
            {
                "21000000-0000-0000-0000-000000000101",
                "21000000-0000-0000-0000-000000000102",
                "21000000-0000-0000-0000-000000000103"
            }) migrationBuilder.DeleteData("EvidenceReferences", "Id", new Guid(id));
        }
    }
}
