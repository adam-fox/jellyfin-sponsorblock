using MediaBrowser.Controller.MediaSegments;
using MediaBrowser.Model.MediaSegments;

namespace Jellyfin.Plugin.SponsorBlock.Orchestration;

/// <summary>
/// Production implementation that forwards to Jellyfin's <see cref="IMediaSegmentManager"/>.
/// </summary>
public sealed class MediaSegmentWriter : IMediaSegmentWriter
{
	private readonly IMediaSegmentManager _manager;

	/// <summary>Initializes the writer with the Jellyfin segment manager.</summary>
	/// <param name="manager">The Jellyfin segment manager (DI-supplied).</param>
	public MediaSegmentWriter(IMediaSegmentManager manager)
	{
		_manager = manager;
	}

	/// <inheritdoc />
	public Task DeleteOwnedAsync(Guid itemId, CancellationToken cancellationToken)
		=> _manager.DeleteSegmentsAsync(itemId, cancellationToken);

	/// <inheritdoc />
	public async Task CreateAsync(MediaSegmentDto segment, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await _manager.CreateSegmentAsync(segment, IMediaSegmentWriter.ProviderName).ConfigureAwait(false);
	}
}
