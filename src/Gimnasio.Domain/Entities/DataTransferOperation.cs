using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class DataTransferOperation : Entity
{
    private static readonly string[] Types = ["backup", "restore"];
    private DataTransferOperation() { }

    public DataTransferOperation(Guid athleteProfileId, string operationType, string sha256, string? fileName, string? safetyBackupSha256, int restored, int alreadyPresent, int conflicts)
    {
        if (athleteProfileId == Guid.Empty) throw new ArgumentException("El perfil es obligatorio.", nameof(athleteProfileId));
        if (!Types.Contains(operationType)) throw new ArgumentException("El tipo de operación no es válido.", nameof(operationType));
        AthleteProfileId = athleteProfileId;
        OperationType = operationType;
        Sha256 = Hash(sha256, nameof(sha256));
        FileName = Clean(fileName, 240);
        SafetyBackupSha256 = string.IsNullOrWhiteSpace(safetyBackupSha256) ? null : Hash(safetyBackupSha256, nameof(safetyBackupSha256));
        Restored = NonNegative(restored, nameof(restored));
        AlreadyPresent = NonNegative(alreadyPresent, nameof(alreadyPresent));
        Conflicts = NonNegative(conflicts, nameof(conflicts));
    }

    public Guid AthleteProfileId { get; private set; }
    public string OperationType { get; private set; } = "backup";
    public string Status { get; private set; } = "completed";
    public string Sha256 { get; private set; } = "";
    public string? SafetyBackupSha256 { get; private set; }
    public string? FileName { get; private set; }
    public int Restored { get; private set; }
    public int AlreadyPresent { get; private set; }
    public int Conflicts { get; private set; }

    private static string Hash(string value, string name) => value.Length == 64 && value.All(Uri.IsHexDigit) ? value.ToLowerInvariant() : throw new ArgumentException("La huella no es válida.", name);
    private static string? Clean(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= max ? value.Trim() : throw new ArgumentException($"El texto admite hasta {max} caracteres.");
    private static int NonNegative(int value, string name) => value >= 0 ? value : throw new ArgumentOutOfRangeException(name);
}
