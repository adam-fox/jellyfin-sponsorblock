using MediaBrowser.Model.MediaSegments;

namespace Jellyfin.Plugin.SponsorBlock.Orchestration;

/// <summary>
/// Plugin-internal abstraction over Jellyfin's <c>IMediaSegmentManager</c>.
/// The orchestrator depends on this; the production implementation forwards to
/// <c>IMediaSegmentManager.CreateSegmentAsync</c> / <c>DeleteSegmentsAsync</c>.
/// </summary>
public interface IMediaSegmentWriter
{
	/// <summary>Constant provider name that owns SponsorBlock-written segments.</summary>
	const string ProviderName = "SponsorBlock";

	/// <summary>
	/// Removes all segments for an item. Note: Jellyfin 10.11's
	/// <c>IMediaSegmentManager.DeleteSegmentsAsync</c> has no per-provider filter, so this
	/// nukes any segment any provider wrote for the item. Acceptable for the SponsorBlock
	/// plugin's scoped (TubeArchivist YouTube) library use case; documented as a caveat.
	/// </summary>
	/// <param name="itemId">The Jellyfin item id.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task DeleteOwnedAsync(Guid itemId, CancellationToken cancellationToken);

	/// <summary>Persists a single SponsorBlock segment.</summary>
	/// <param name="segment">The segment DTO to persist.</param>
	/// <param name="cancellationToken">Cancellation token (advisory; the underlying API does not accept one).</param>
	Task CreateAsync(MediaSegmentDto segment, CancellationToken cancellationToken);
}
