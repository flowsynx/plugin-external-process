namespace FlowSynx.Plugins.ExternalProcess.Models;

internal class InputParameter
{
    public string FileName { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string? WorkingDirectory { get; set; }
    public bool ShowWindow { get; set; } = false;
    public bool FailOnNonZeroExit { get; set; } = true;
}