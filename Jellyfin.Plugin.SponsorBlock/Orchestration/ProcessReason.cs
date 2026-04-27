namespace Jellyfin.Plugin.SponsorBlock.Orchestration;

/// <summary>
/// Why <see cref="SponsorBlockOrchestrator.ProcessAsync"/> was invoked.
/// Drives the decision-table branches inside the orchestrator.
/// </summary>
public enum ProcessReason
{
	/// <summary>Library item was just added (ILibraryManager.ItemAdded).</summary>
	ItemAdded,

	/// <summary>A session started playback (ISessionManager.PlaybackStart).</summary>
	PlaybackStart,

	/// <summary>The daily refresh task is running.</summary>
	DailyScan,
}
