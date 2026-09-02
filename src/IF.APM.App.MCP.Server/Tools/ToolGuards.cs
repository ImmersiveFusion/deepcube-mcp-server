using System.Text.Json;
using System.Text.RegularExpressions;

namespace IF.APM.App.MCP.Server.Tools;

/// <summary>
/// Input validation and time handling shared by every tool.
/// Mirrors the Unity client's ApmToolContext helpers so both surfaces
/// reject the same inputs and send the same instants to the API.
/// </summary>
internal static partial class ToolGuards
{
    internal static readonly TimeSpan MaxTimeRange = TimeSpan.FromDays(7);

    [GeneratedRegex("^[0-9a-fA-F]{1,64}$")]
    private static partial Regex HexIdPattern();

    /// <summary>
    /// Serialises a caller-safe error payload. Internal exception detail never
    /// reaches the MCP client.
    /// </summary>
    internal static string Error(string message) =>
        JsonSerializer.Serialize(new { error = message });

    internal static string? ValidateHexId(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value) || !HexIdPattern().IsMatch(value)
            ? Error($"Invalid {paramName} format. Expected a hex string.")
            : null;

    internal static string? ValidateTimeRange(DateTimeOffset rangeStart, DateTimeOffset rangeEnd)
    {
        if (rangeEnd <= rangeStart)
            return Error("rangeEnd must be after rangeStart.");
        if (rangeEnd - rangeStart > MaxTimeRange)
            return Error($"Time range exceeds maximum of {MaxTimeRange.TotalDays} days.");
        return null;
    }

    /// <summary>
    /// Normalises an instant to UTC before it reaches the generated API client.
    /// The NSwag client formats dates with ToString("s"), which silently strips
    /// the timezone offset, so a non-UTC offset would otherwise be read by the
    /// API as a different instant. The Unity client does the same thing in
    /// ApmToolContext.ToUtc.
    /// </summary>
    internal static DateTimeOffset ToUtc(DateTimeOffset value) => new(value.UtcDateTime, TimeSpan.Zero);

    internal static DateTimeOffset? ToUtc(DateTimeOffset? value) => value is null ? null : ToUtc(value.Value);
}
