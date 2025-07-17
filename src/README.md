## FlowSynx External Process Plugin

The **FlowSynx External Process Plugin** is a plug-and-play integration for the FlowSynx automation engine. It allows workflows to execute external scripts, commands, or programs on the host system and capture their output, with no custom coding required.

This plugin is automatically installed by the FlowSynx engine when selected in the workflow builder. It is not intended for standalone developer usage outside the FlowSynx platform.

---

## Purpose

The External Process Plugin enables FlowSynx users to:

- Execute external scripts, programs, or shell commands.  
- Pass input arguments to processes dynamically from workflows.  
- Capture standard output (`stdout`) and standard error (`stderr`) for further processing.  
- Optionally fail the workflow if the executed process returns a non-zero exit code.  

It is ideal for integrating custom logic, legacy tools, or third-party utilities directly into FlowSynx workflows.

---

## Input Parameters

The plugin accepts the following parameters:
 
- `FileName` (string): **Required.** The name or full path of the executable or script to run (e.g., `python`, `bash`, `cmd.exe`, `mytool.exe`).  
- `Arguments` (string): **Optional.** Command-line arguments to pass to the process.  
- `WorkingDirectory` (string): **Optional.** The directory where the process should execute.  
- `ShowWindow` (boolean): **Optional.** Whether to display the process window (default: `false`).  
- `FailOnNonZeroExit` (boolean): **Optional.** If `true`, the plugin will throw an error when the process exits with a non-zero code (default: `true`).  

### Example

**Input Parameters:**

```json
{
  "FileName": "echo",
  "Arguments": "Hello, FlowSynx!"
}
```

**Output:**

```json
{
  "Id": "process-1234",
  "SourceType": "Process",
  "Format": "Text",
  "Metadata": {
    "ExitCode": 0,
    "FileName": "echo",
    "Arguments": "Hello, FlowSynx!",
    "WorkingDirectory": "C:\Users\flow",
    "ExecutionTime": "2025-07-17T12:34:56Z"
  },
  "Content": "Hello, FlowSynx!",
  "RawData": null,
  "StructuredData": null
}
```

---

## Example Use Case in FlowSynx

1. Add the External Process Plugin to your FlowSynx workflow.  
3. Specify the `FileName` and optional `Arguments`.  
4. Use captured output in downstream workflow steps (e.g., parsing `stdout` into variables).  

---

## Debugging Tips

- If the process fails with a non-zero exit code and `FailOnNonZeroExit` is enabled, the plugin will raise an error. Set `FailOnNonZeroExit` to `false` to handle failures gracefully.  
- Use absolute paths for `FileName` if the executable is not in the system `PATH`.  
- Capture both `stdout` and `stderr` to troubleshoot unexpected process behavior.  

---

## Security Notes

- Processes are executed in the host system environment where FlowSynx is deployed.  
- Ensure only trusted executables/scripts are configured to avoid security risks.  
- All process executions respect FlowSynx platform permissions and isolation rules.  

---

## License

© FlowSynx. All rights reserved.