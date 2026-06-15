namespace ITStockM.Services;

public sealed class AiModelRetrainingOptions
{
    public const string Section = "AiModels:Retraining";

    public bool Enabled { get; set; } = true;
    public int InitialDelayMinutes { get; set; } = 3;
    public int IntervalMinutes { get; set; } = 360;
    public int MinimumHardwareTrainingSamples { get; set; } = 6;
    public int MinimumTicketTrainingSamples { get; set; } = 12;
    public double ActivationMetricTolerance { get; set; } = 0.02;
    public double MinimumHardwareValidationMetric { get; set; } = 0.70;
    public double MinimumTicketValidationMetric { get; set; } = 0.70;
    public double MinimumHardwarePositiveRate { get; set; } = 0.10;
    public double MaximumHardwarePositiveRate { get; set; } = 0.90;
    public int MinimumTicketDistinctLabels { get; set; } = 3;
    public double MaximumTicketMissingTextRate { get; set; } = 0.05;
    public int MaxModelVersionsToKeep { get; set; } = 8;
    public double DriftMediumThreshold { get; set; } = 35;
    public double DriftHighThreshold { get; set; } = 60;

    // Drift-triggered retraining policy
    public bool ForceRetrainOnHighDrift { get; set; } = true;
    public int DriftHighConsecutiveCyclesToForceRetrain { get; set; } = 2;
}
