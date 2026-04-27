namespace Jellyfin.Plugin.SponsorBlock.Reset;

/// <summary>
/// Wipes SponsorBlock state and owned media segments for items in the configured library scope,
/// returning the items to a "never seen" state so the orchestrator re-fetches on next playback.
/// </summary>
public interface IResetService
{
	/// <summary>
	/// Iterates every <see cref="MediaBrowser.Controller.Entities.Video"/> in the configured
	/// <c>EnabledLibraryIds</c> and, for each, deletes its owned segments and its state row.
	/// Returns the number of items processed.
	/// </summary>
	Task<int> ResetScopedAsync(CancellationToken cancellationToken);
}
