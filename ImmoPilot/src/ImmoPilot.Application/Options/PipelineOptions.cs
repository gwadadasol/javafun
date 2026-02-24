namespace ImmoPilot.Application.Options;

public class PipelineOptions
{
    public const string SectionName = "Pipeline";
    public bool DryRun { get; set; }
    public string RunIdPrefix { get; set; } = "Run";
}
