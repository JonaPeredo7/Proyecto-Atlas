using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class RecurringScheduleBlock : Entity
{
    private RecurringScheduleBlock() { }

    public RecurringScheduleBlock(Guid athleteProfileId, int dayOfWeek, string name, string category)
    {
        AthleteProfileId = athleteProfileId;
        DayOfWeek = dayOfWeek is >= 1 and <= 7 ? dayOfWeek : throw new ArgumentOutOfRangeException(nameof(dayOfWeek));
        Name = Required(name);
        Category = Required(category);
    }

    public Guid AthleteProfileId { get; private set; }
    public int DayOfWeek { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string TimeWindow { get; private set; } = "exact";
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string? Notes { get; private set; }

    public void Configure(string timeWindow, TimeOnly? startTime, TimeOnly? endTime, DateOnly effectiveFrom, DateOnly? effectiveTo, string? notes)
    {
        TimeWindow = Allowed(timeWindow);
        if (effectiveTo < effectiveFrom) throw new ArgumentException("La vigencia no puede finalizar antes de comenzar.");
        if (TimeWindow == "exact" && (startTime is null || endTime is null || endTime <= startTime))
            throw new ArgumentException("Un bloque horario exacto necesita una hora de inicio y otra de fin válidas.");
        if (TimeWindow != "exact") { startTime = null; endTime = null; }
        StartTime = startTime;
        EndTime = endTime;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        Notes = Clean(notes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }

    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("El valor es obligatorio.") : value.Trim();
    private static string Allowed(string value) => value is "exact" or "morning" or "afternoon" or "evening" or "flexible" ? value : throw new ArgumentException("La franja horaria no es válida.");
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
