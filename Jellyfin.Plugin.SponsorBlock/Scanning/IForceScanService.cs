namespace Jellyfin.Plugin.SponsorBlock.Scanning;

/// <summary>
/// Starts and reports the one-shot full-library SponsorBlock backfill scan.
/// </summary>
public interface IForceScanService
{
	/// <summary>Starts a full scan of all videos in the selected libraries unless one is already running.</summary>
	ForceScanStartResponse StartScanAll();

	/// <summary>Returns the current scan status.</summary>
	ForceScanStatus GetStatus();
}

/// <summary>Response for a force-scan start request.</summary>
/// <param name="Started">Whether this request started a new scan.</param>
/// <param name="AlreadyRunning">Whether an existing scan is already running.</param>
/// <param name="Status">Current scan status.</param>
public sealed record ForceScanStartResponse(bool Started, bool AlreadyRunning, ForceScanStatus Status);

/// <summary>Current force-scan status.</summary>
/// <param name="IsRunning">Whether a force scan is currently running.</param>
/// <param name="LastStartedAt">UTC time at which the last scan started.</param>
/// <param name="LastCompletedAt">UTC time at which the last scan completed.</param>
/// <param name="LastItemsProcessed">Number of items processed by the last completed scan.</param>
public sealed record ForceScanStatus(
	bool IsRunning,
	DateTimeOffset? LastStartedAt,
	DateTimeOffset? LastCompletedAt,
	int LastItemsProcessed);
