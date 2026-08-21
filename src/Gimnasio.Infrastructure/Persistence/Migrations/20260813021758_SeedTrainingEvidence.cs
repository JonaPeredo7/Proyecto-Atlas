using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gimnasio.Infrastructure.Persistence.Migrations;

public partial class SeedTrainingEvidence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "EvidenceReferences",
            columns: new[] { "Id", "Topic", "Title", "PermanentId", "SourceUrl", "Level", "Status", "PublishedOn", "NextReviewOn", "Applicability", "Limitations", "CreatedAt", "UpdatedAt", "IsActive" },
            values: new object[,]
            {
                { new Guid("21000000-0000-0000-0000-000000000001"), "Carga interna", "A new approach to monitoring exercise training", "PMID:11708692", "https://pubmed.ncbi.nlm.nih.gov/11708692/", 2, "operational", new DateOnly(2001, 2, 1), new DateOnly(2027, 8, 1), "Sustenta el uso descriptivo de duración de la sesión multiplicada por RPE.", "Es una estimación de carga interna, no una medición mecánica ni un predictor individual de lesión.", new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero), null, true },
                { new Guid("21000000-0000-0000-0000-000000000002"), "Bienestar", "Single-Item Self-Report Measures of Team-Sport Athlete Wellbeing and Their Relationship With Training Load: A Systematic Review", "PMID:32991706", "https://pubmed.ncbi.nlm.nih.gov/32991706/", 4, "informative", new DateOnly(2020, 9, 30), new DateOnly(2027, 8, 1), "Sustenta registrar sueño, fatiga, estrés, energía y dolor como contexto subjetivo repetido.", "Las escalas breves no diagnostican ni deben interpretarse con umbrales universales.", new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero), null, true },
                { new Guid("21000000-0000-0000-0000-000000000003"), "Carga interna", "IOC consensus statement on load in sport and risk of illness", "DOI:10.1136/bjsports-2016-096572", "https://bjsm.bmj.com/content/50/17/1043", 5, "informative", new DateOnly(2016, 8, 1), new DateOnly(2027, 8, 1), "Sustenta integrar carga deportiva, carga no deportiva, bienestar y síntomas en el seguimiento.", "La evidencia varía entre deportes y poblaciones; no ofrece una dosis individual automática.", new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero), null, true },
                { new Guid("21000000-0000-0000-0000-000000000004"), "Predicción de lesión", "Acute:Chronic Workload Ratio: Conceptual Issues and Fundamental Pitfalls", "PMID:32502973", "https://pubmed.ncbi.nlm.nih.gov/32502973/", 2, "operational", new DateOnly(2020, 6, 5), new DateOnly(2027, 8, 1), "Justifica no presentar el cociente agudo:crónico como predictor ni regla de prescripción.", "Es un análisis metodológico crítico; Atlas mantiene comparaciones descriptivas sin inferir causalidad.", new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero), null, true }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var id in new[]
        {
            "21000000-0000-0000-0000-000000000001", "21000000-0000-0000-0000-000000000002",
            "21000000-0000-0000-0000-000000000003", "21000000-0000-0000-0000-000000000004"
        }) migrationBuilder.DeleteData("EvidenceReferences", "Id", new Guid(id));
    }
}
