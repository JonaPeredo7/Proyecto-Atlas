using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class TrainingExercise : Entity
{
    private TrainingExercise() { }

    public TrainingExercise(Guid trainingSessionId, int order, string name)
    {
        TrainingSessionId = trainingSessionId;
        Order = order > 0 ? order : throw new ArgumentOutOfRangeException(nameof(order));
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("El ejercicio es obligatorio.", nameof(name)) : name.Trim();
    }

    public Guid TrainingSessionId { get; private set; }
    public int Order { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Category { get; private set; }
    public int PlannedSets { get; private set; }
    public string PlannedRepetitions { get; private set; } = string.Empty;
    public decimal? PlannedLoadKg { get; private set; }
    public int? RestSeconds { get; private set; }
    public string? Notes { get; private set; }
    public int? CompletedSets { get; private set; }
    public string? ActualRepetitions { get; private set; }
    public decimal? ActualLoadKg { get; private set; }
    public int? ExerciseRpe { get; private set; }
    public bool IsCompleted { get; private set; }

    public void Prescribe(string? category, int sets, string repetitions, decimal? loadKg, int? restSeconds, string? notes)
    {
        if (sets is < 1 or > 30) throw new ArgumentOutOfRangeException(nameof(sets));
        if (string.IsNullOrWhiteSpace(repetitions)) throw new ArgumentException("Las repeticiones son obligatorias.", nameof(repetitions));
        if (loadKg is < 0 or > 1000) throw new ArgumentOutOfRangeException(nameof(loadKg));
        if (restSeconds is < 0 or > 3600) throw new ArgumentOutOfRangeException(nameof(restSeconds));
        Category = Clean(category);
        PlannedSets = sets;
        PlannedRepetitions = repetitions.Trim();
        PlannedLoadKg = loadKg;
        RestSeconds = restSeconds;
        Notes = Clean(notes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Rename(string name) => Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("El ejercicio es obligatorio.", nameof(name)) : name.Trim();

    public void Record(int completedSets, string? actualRepetitions, decimal? actualLoadKg, int? exerciseRpe)
    {
        if (completedSets is < 0 or > 30) throw new ArgumentOutOfRangeException(nameof(completedSets));
        if (actualLoadKg is < 0 or > 1000) throw new ArgumentOutOfRangeException(nameof(actualLoadKg));
        if (exerciseRpe is < 1 or > 10) throw new ArgumentOutOfRangeException(nameof(exerciseRpe));
        CompletedSets = completedSets;
        ActualRepetitions = Clean(actualRepetitions);
        ActualLoadKg = actualLoadKg;
        ExerciseRpe = exerciseRpe;
        IsCompleted = completedSets > 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
