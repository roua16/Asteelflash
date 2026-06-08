namespace ITStockM.Services;

public sealed class AiModelRetrainingOptions
{
    public const string Section = "AiModels:Retraining";

    public bool Enabled { get; set; } = true;
    public int InitialDelayMinutes { get; set; } = 3;
    public int IntervalMinutes { get; set; } = 360;
    public int MinimumHardwareTrainingSamples { get; set; } = 12;
    public int MinimumTicketTrainingSamples { get; set; } = 24;
    public double ActivationMetricTolerance { get; set; } = 0.02;
    public int MaxModelVersionsToKeep { get; set; } = 8;
    public double DriftMediumThreshold { get; set; } = 35;
    public double DriftHighThreshold { get; set; } = 60;
}
