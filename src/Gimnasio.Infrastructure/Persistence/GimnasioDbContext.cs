using Gimnasio.Domain.Entities;
using Gimnasio.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Infrastructure.Persistence;

public sealed class GimnasioDbContext(DbContextOptions<GimnasioDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<AthleteProfile> AthleteProfiles => Set<AthleteProfile>();
    public DbSet<ProfileFact> ProfileFacts => Set<ProfileFact>();
    public DbSet<DailyCheckIn> DailyCheckIns => Set<DailyCheckIn>();
    public DbSet<DailyActivity> DailyActivities => Set<DailyActivity>();
    public DbSet<KneeCheck> KneeChecks => Set<KneeCheck>();
    public DbSet<PersonalGoal> PersonalGoals => Set<PersonalGoal>(); public DbSet<TrainingCycle> TrainingCycles => Set<TrainingCycle>(); public DbSet<PlanChange> PlanChanges => Set<PlanChange>();
    public DbSet<LearningEntry> LearningEntries => Set<LearningEntry>();
    public DbSet<EvidenceReference> EvidenceReferences => Set<EvidenceReference>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<TrainingExercise> TrainingExercises => Set<TrainingExercise>();
    public DbSet<TrainingFollowUp> TrainingFollowUps => Set<TrainingFollowUp>();
    public DbSet<MetricDefinition> MetricDefinitions => Set<MetricDefinition>();
    public DbSet<MeasurementEntry> MeasurementEntries => Set<MeasurementEntry>();
    public DbSet<ReportShare> ReportShares => Set<ReportShare>();
    public DbSet<ReportFeedback> ReportFeedback => Set<ReportFeedback>();
    public DbSet<DataTransferOperation> DataTransferOperations => Set<DataTransferOperation>();
    public DbSet<DailyPlanDecision> DailyPlanDecisions => Set<DailyPlanDecision>();
    public DbSet<RecurringScheduleBlock> RecurringScheduleBlocks => Set<RecurringScheduleBlock>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        ConfigureIdentity(builder);
        ConfigureAtlas(builder);
        ConfigureTraining(builder);
        ConfigureMeasurements(builder);
        ConfigurePlanning(builder);
        ConfigureLearning(builder);
        ConfigureReports(builder);
        ConfigureDataTransfer(builder);
        ConfigureDailyDecisions(builder);
        ConfigureRecurringSchedule(builder);
    }
    private static void ConfigureRecurringSchedule(ModelBuilder builder)
    {
        builder.Entity<RecurringScheduleBlock>(entity =>
        {
            entity.ToTable("RecurringScheduleBlocks"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired(); entity.Property(x => x.Category).HasMaxLength(60).IsRequired();
            entity.Property(x => x.TimeWindow).HasMaxLength(20).IsRequired(); entity.Property(x => x.Notes).HasMaxLength(600);
            entity.HasIndex(x => new { x.AthleteProfileId, x.DayOfWeek, x.EffectiveFrom });
            entity.HasOne<AthleteProfile>().WithMany().HasForeignKey(x => x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        });
    }
    private static void ConfigureDailyDecisions(ModelBuilder builder)
    {
        builder.Entity<DailyPlanDecision>(entity=>
        {
            entity.ToTable("DailyPlanDecisions");entity.HasKey(x=>x.Id);entity.Property(x=>x.Decision).HasMaxLength(30).IsRequired();entity.Property(x=>x.Reason).HasMaxLength(1000).IsRequired();entity.Property(x=>x.ContextStatus).HasMaxLength(20).IsRequired();entity.HasIndex(x=>new{x.AthleteProfileId,x.Date}).IsUnique();entity.HasOne<AthleteProfile>().WithMany().HasForeignKey(x=>x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        });
    }
    private static void ConfigureDataTransfer(ModelBuilder builder)
    {
        builder.Entity<DataTransferOperation>(entity =>
        {
            entity.ToTable("DataTransferOperations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OperationType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SafetyBackupSha256).HasMaxLength(64);
            entity.Property(x => x.FileName).HasMaxLength(240);
            entity.HasIndex(x => new { x.AthleteProfileId, x.CreatedAt });
            entity.HasOne<AthleteProfile>().WithMany().HasForeignKey(x => x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        });
    }
    private static void ConfigureReports(ModelBuilder builder)
    {
        builder.Entity<ReportShare>(entity =>
        {
            entity.ToTable("ReportShares");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SnapshotJson).IsRequired();
            entity.Property(x => x.RecipientLabel).HasMaxLength(160);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.AthleteProfileId, x.CreatedAt });
            entity.HasOne<AthleteProfile>().WithMany().HasForeignKey(x => x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<ReportFeedback>(entity =>
        {
            entity.ToTable("ReportFeedback");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AuthorName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Kind).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Section).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1600).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.DecisionNote).HasMaxLength(1000);
            entity.HasIndex(x => new { x.ReportShareId, x.CreatedAt });
            entity.HasOne<ReportShare>().WithMany().HasForeignKey(x => x.ReportShareId).OnDelete(DeleteBehavior.Cascade);
        });
    }
    private static void ConfigureLearning(ModelBuilder builder)
    {
        builder.Entity<LearningEntry>(e=>{e.ToTable("LearningEntries");e.HasKey(x=>x.Id);e.Property(x=>x.Title).HasMaxLength(180).IsRequired();e.Property(x=>x.Category).HasMaxLength(80).IsRequired();e.Property(x=>x.Observation).HasMaxLength(1600).IsRequired();e.Property(x=>x.Interpretation).HasMaxLength(1600);e.Property(x=>x.NextAction).HasMaxLength(1000);e.Property(x=>x.Confidence).HasMaxLength(20).IsRequired();e.Property(x=>x.Status).HasMaxLength(20).IsRequired();e.Property(x=>x.FollowUpOutcome).HasMaxLength(30);e.Property(x=>x.FollowUpObservation).HasMaxLength(1600);e.HasIndex(x=>new{x.AthleteProfileId,x.Date});e.HasOne<AthleteProfile>().WithMany().HasForeignKey(x=>x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);e.HasOne<TrainingSession>().WithMany().HasForeignKey(x=>x.TrainingSessionId).OnDelete(DeleteBehavior.NoAction);e.HasOne<PersonalGoal>().WithMany().HasForeignKey(x=>x.PersonalGoalId).OnDelete(DeleteBehavior.NoAction);e.HasOne<TrainingCycle>().WithMany().HasForeignKey(x=>x.TrainingCycleId).OnDelete(DeleteBehavior.NoAction);});
    }
    private static void ConfigurePlanning(ModelBuilder builder)
    {
        builder.Entity<PersonalGoal>(e=>{e.ToTable("PersonalGoals");e.HasKey(x=>x.Id);e.Property(x=>x.Title).HasMaxLength(180).IsRequired();e.Property(x=>x.Category).HasMaxLength(80).IsRequired();e.Property(x=>x.Unit).HasMaxLength(40);e.Property(x=>x.BaselineValue).HasPrecision(12,3);e.Property(x=>x.TargetValue).HasPrecision(12,3);e.Property(x=>x.Status).HasMaxLength(30).IsRequired();e.Property(x=>x.Rationale).HasMaxLength(1000);e.HasIndex(x=>new{x.AthleteProfileId,x.Status});e.HasOne<AthleteProfile>().WithMany().HasForeignKey(x=>x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);e.HasOne<MetricDefinition>().WithMany().HasForeignKey(x=>x.MetricDefinitionId).OnDelete(DeleteBehavior.NoAction);});
        builder.Entity<TrainingCycle>(e=>{e.ToTable("TrainingCycles");e.HasKey(x=>x.Id);e.Property(x=>x.Name).HasMaxLength(180).IsRequired();e.Property(x=>x.Focus).HasMaxLength(500).IsRequired();e.Property(x=>x.Status).HasMaxLength(30).IsRequired();e.Property(x=>x.Notes).HasMaxLength(1000);e.HasIndex(x=>new{x.AthleteProfileId,x.StartDate});e.HasOne<AthleteProfile>().WithMany().HasForeignKey(x=>x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);});
        builder.Entity<PlanChange>(e=>{e.ToTable("PlanChanges");e.HasKey(x=>x.Id);e.Property(x=>x.EntityType).HasMaxLength(30).IsRequired();e.Property(x=>x.Reason).HasMaxLength(500).IsRequired();e.Property(x=>x.Summary).HasMaxLength(600).IsRequired();e.HasIndex(x=>new{x.AthleteProfileId,x.CreatedAt});e.HasOne<AthleteProfile>().WithMany().HasForeignKey(x=>x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);});
    }

    private static void ConfigureMeasurements(ModelBuilder builder)
    {
        builder.Entity<MetricDefinition>(entity =>
        {
            entity.ToTable("MetricDefinitions"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired(); entity.Property(x => x.Category).HasMaxLength(80).IsRequired(); entity.Property(x => x.Unit).HasMaxLength(40).IsRequired();
            entity.Property(x => x.TargetValue).HasPrecision(12, 3); entity.Property(x => x.Protocol).HasMaxLength(1200); entity.Property(x => x.SourceTitle).HasMaxLength(400); entity.Property(x => x.SourceUrl).HasMaxLength(1000);
            entity.HasIndex(x => new { x.AthleteProfileId, x.Name }).IsUnique().HasFilter("[IsActive] = 1");
            entity.HasOne<AthleteProfile>().WithMany().HasForeignKey(x => x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<MeasurementEntry>(entity =>
        {
            entity.ToTable("MeasurementEntries"); entity.HasKey(x => x.Id); entity.Property(x => x.Value).HasPrecision(12, 3); entity.Property(x => x.Conditions).HasMaxLength(500); entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.MetricDefinitionId, x.Date }).IsUnique();
            entity.HasOne<MetricDefinition>().WithMany().HasForeignKey(x => x.MetricDefinitionId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTraining(ModelBuilder builder)
    {
        builder.Entity<TrainingSession>(entity =>
        {
            entity.ToTable("TrainingSessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.ActivityType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Goal).HasMaxLength(600);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.CompletionNotes).HasMaxLength(1000);
            entity.Property(x => x.Version).HasDefaultValue(1);
            entity.HasOne<PersonalGoal>().WithMany().HasForeignKey(x=>x.PersonalGoalId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<TrainingCycle>().WithMany().HasForeignKey(x=>x.TrainingCycleId).OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(x => new { x.AthleteProfileId, x.Date });
            entity.HasOne<AthleteProfile>().WithMany().HasForeignKey(x => x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TrainingExercise>(entity =>
        {
            entity.ToTable("TrainingExercises");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(80);
            entity.Property(x => x.PlannedRepetitions).HasMaxLength(80).IsRequired();
            entity.Property(x => x.ActualRepetitions).HasMaxLength(80);
            entity.Property(x => x.PlannedLoadKg).HasPrecision(8, 2);
            entity.Property(x => x.ActualLoadKg).HasPrecision(8, 2);
            entity.Property(x => x.Notes).HasMaxLength(800);
            entity.HasIndex(x => new { x.TrainingSessionId, x.Order }).IsUnique();
            entity.HasOne<TrainingSession>().WithMany().HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TrainingFollowUp>(entity =>
        {
            entity.ToTable("TrainingFollowUps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PainLocation).HasMaxLength(100);
            entity.Property(x => x.Stiffness).HasMaxLength(30);
            entity.Property(x => x.Swelling).HasMaxLength(30);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => x.TrainingSessionId).IsUnique();
            entity.HasOne<TrainingSession>().WithOne().HasForeignKey<TrainingFollowUp>(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(80);
            entity.Property(x => x.LastName).HasMaxLength(80);
        });

    }

    private static void ConfigureAtlas(ModelBuilder builder)
    {
        builder.Entity<AthleteProfile>(entity =>
        {
            entity.ToTable("AthleteProfiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisplayName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.HeightCm).HasPrecision(5, 2);
            entity.Property(x => x.ReferenceWeightKg).HasPrecision(6, 2);
            entity.Property(x => x.PrimaryGoal).HasMaxLength(600);
            entity.Property(x => x.DominantHand).HasMaxLength(40);
            entity.Property(x => x.DominantLeg).HasMaxLength(40);
            entity.Property(x => x.AffectedKnee).HasMaxLength(40);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasOne<ApplicationUser>().WithOne().HasForeignKey<AthleteProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProfileFact>(entity =>
        {
            entity.ToTable("ProfileFacts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Category).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(1000);
            entity.Property(x => x.SourceTitle).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.AthleteProfileId, x.Category });
            entity.HasOne<AthleteProfile>().WithMany().HasForeignKey(x => x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DailyCheckIn>(entity =>
        {
            entity.ToTable("DailyCheckIns");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PainLocation).HasMaxLength(100);
            entity.Property(x => x.PainSide).HasMaxLength(30);
            entity.Property(x => x.Stiffness).HasMaxLength(30);
            entity.Property(x => x.Swelling).HasMaxLength(30);
            entity.Property(x => x.PlannedCyclingKm).HasPrecision(6, 2);
            entity.Property(x => x.PlannedActivity).HasMaxLength(200);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.AthleteProfileId, x.Date }).IsUnique();
            entity.HasOne<AthleteProfile>().WithMany().HasForeignKey(x => x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DailyActivity>(entity =>
        {
            entity.ToTable("DailyActivities");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ActivityType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.DistanceKm).HasPrecision(7, 2);
            entity.Property(x => x.Notes).HasMaxLength(800);
            entity.Property(x => x.PlannedSource).HasMaxLength(160);
            entity.Property(x => x.WorkDemands).HasMaxLength(300);
            entity.Property(x => x.UnusualConditions).HasMaxLength(400);
            entity.Ignore(x => x.InternalLoad);
            entity.HasIndex(x => new { x.AthleteProfileId, x.Date });
            entity.HasOne<AthleteProfile>().WithMany().HasForeignKey(x => x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<KneeCheck>(entity =>
        {
            entity.ToTable("KneeChecks"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Context).HasMaxLength(120).IsRequired(); entity.Property(x => x.Side).HasMaxLength(30).IsRequired(); entity.Property(x => x.Swelling).HasMaxLength(30).IsRequired(); entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.AthleteProfileId, x.RecordedAt });
            entity.HasOne<AthleteProfile>().WithMany().HasForeignKey(x => x.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EvidenceReference>(entity =>
        {
            entity.ToTable("EvidenceReferences");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Topic).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(400).IsRequired();
            entity.Property(x => x.PermanentId).HasMaxLength(120);
            entity.Property(x => x.SourceUrl).HasMaxLength(1000);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Applicability).HasMaxLength(1000);
            entity.Property(x => x.Limitations).HasMaxLength(1600);
            entity.HasIndex(x => new { x.Topic, x.Status });
        });
    }
}
