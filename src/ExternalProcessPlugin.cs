using FlowSynx.PluginCore;
using FlowSynx.PluginCore.Extensions;
using FlowSynx.PluginCore.Helpers;
using FlowSynx.Plugins.ExternalProcess.Models;
using System.Diagnostics;
using System.Text;

namespace FlowSynx.Plugins.ExternalProcess;

public class ExternalProcessPlugin : IPlugin
{
    private IPluginLogger? _logger;
    private bool _isInitialized;

    public PluginMetadata Metadata => new()
    {
        Id = Guid.Parse("df657e78-10ae-4e61-beda-2e4f9b1b6a7a"),
        Name = "ExternalProcess",
        CompanyName = "FlowSynx",
        Description = Resources.PluginDescription,
        Version = new Version(1, 0, 0),
        Category = PluginCategory.Execution,
        Authors = new List<string> { "FlowSynx" },
        Copyright = "© FlowSynx. All rights reserved.",
        Icon = "flowsynx.png",
        ReadMe = "README.md",
        RepositoryUrl = "https://github.com/flowsynx/plugin-external-process",
        ProjectUrl = "https://flowsynx.io",
        Tags = new List<string>() { "flowsynx", "execution", "external-process", "plugin" },
        MinimumFlowSynxVersion = new Version(1, 1, 1),
    };

    public PluginSpecifications? Specifications { get; set; }

    public Type SpecificationsType => typeof(ExternalProcessPluginSpecifications);

    public IReadOnlyCollection<string> SupportedOperations => new List<string>();

    public Task Initialize(IPluginLogger logger)
    {
        if (ReflectionHelper.IsCalledViaReflection())
            throw new InvalidOperationException(Resources.ReflectionBasedAccessIsNotAllowed);

        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _isInitialized = true;
        return Task.CompletedTask;
    }

    public async Task<object?> ExecuteAsync(PluginParameters parameters, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (ReflectionHelper.IsCalledViaReflection())
            throw new InvalidOperationException(Resources.ReflectionBasedAccessIsNotAllowed);

        if (!_isInitialized)
            throw new InvalidOperationException($"Plugin '{Metadata.Name}' v{Metadata.Version} is not initialized.");

        var inputParameter = parameters.ToObject<InputParameter>();
        return await ExecuteProcessAsync(inputParameter, cancellationToken);
    }

    #region private methods
    private async Task<PluginContext> ExecuteProcessAsync(
        InputParameter inputParameter,
        CancellationToken cancellationToken)
    {
        if (_logger == null)
        {
            throw new InvalidOperationException("Plugin is not initialized. Call Initialize() first.");
        }

        _logger.LogInfo($"Starting process: {inputParameter.FileName} {inputParameter.Arguments}");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = inputParameter.FileName,
            Arguments = inputParameter.Arguments,
            WorkingDirectory = string.IsNullOrWhiteSpace(inputParameter.WorkingDirectory)
                ? Environment.CurrentDirectory
                : inputParameter.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = !inputParameter.ShowWindow
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                outputBuilder.AppendLine(e.Data);
                _logger.LogDebug($"STDOUT: {e.Data}");
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
                _logger.LogError($"STDERR: {e.Data}");
            }
        };

        var startTime = DateTime.UtcNow;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await Task.Run(() =>
        {
            while (!process.HasExited)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(100);
            }
        }, cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        var endTime = DateTime.UtcNow;

        var stdout = outputBuilder.ToString();
        var stderr = errorBuilder.ToString();
        var combinedOutput = stdout + stderr;

        if (process.ExitCode != 0)
        {
            _logger.LogError($"Process failed with exit code {process.ExitCode}");
            if (inputParameter.FailOnNonZeroExit)
                throw new Exception($"Process failed with exit code {process.ExitCode}. Error: {stderr}");
            else
                _logger.LogWarning("Continuing despite process failure because FailOnNonZeroExit is false.");
        }
        else
        {
            _logger.LogInfo($"Process completed successfully with exit code {process.ExitCode}");
        }

        var context = new PluginContext(Guid.NewGuid().ToString(), "Execution")
        {
            Format = "Text",
            Content = combinedOutput,
            StructuredData = null
        };
        context.Metadata["ExitCode"] = process.ExitCode;
        context.Metadata["StartTimeUtc"] = startTime;
        context.Metadata["EndTimeUtc"] = endTime;
        context.Metadata["Duration"] = endTime - startTime;
        context.Metadata["WasSuccessful"] = process.ExitCode == 0;
        context.Metadata["StdErrPresent"] = !string.IsNullOrWhiteSpace(stderr);

        return context;
    }
    #endregion
}