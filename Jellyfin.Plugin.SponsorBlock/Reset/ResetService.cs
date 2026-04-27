using Jellyfin.Plugin.SponsorBlock.Configuration;
using Jellyfin.Plugin.SponsorBlock.Orchestration;
using Jellyfin.Plugin.SponsorBlock.State;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SponsorBlock.Reset;

/// <summary>
/// Default implementation of <see cref="IResetService"/>.
/// </summary>
public sealed class ResetService : IResetService
{
	private readonly IMediaSegmentWriter _writer;
	private readonly ISponsorBlockStateStore _store;
	private readonly Func<PluginConfiguration> _configAccessor;
	private readonly Func<Guid[], IEnumerable<Video>> _scopedVideos;
	private readonly ILogger<ResetService> _logger;

	/// <summary>
	/// Production constructor — enumerates videos via <see cref="ILibraryManager.GetItemList(InternalItemsQuery)"/>.
	/// </summary>
	/// <param name="libraryManager">Jellyfin library manager.</param>
	/// <param name="writer">Wrapper around the Jellyfin segment manager.</param>
	/// <param name="store">Per-item state store.</param>
	/// <param name="configAccessor">Returns the current plugin configuration.</param>
	/// <param name="logger">Logger.</param>
	public ResetService(
		ILibraryManager libraryManager,
		IMediaSegmentWriter writer,
		ISponsorBlockStateStore store,
		Func<PluginConfiguration> configAccessor,
		ILogger<ResetService> logger)
		: this(writer, store, configAccessor, ids => EnumerateScoped(libraryManager, ids), logger)
	{
	}

	/// <summary>
	/// Test constructor — accepts an injected video enumerator so tests don't need a real ILibraryManager.
	/// </summary>
	internal ResetService(
		IMediaSegmentWriter writer,
		ISponsorBlockStateStore store,
		Func<PluginConfiguration> configAccessor,
		Func<Guid[], IEnumerable<Video>> scopedVideos,
		ILogger<ResetService> logger)
	{
		_writer = writer;
		_store = store;
		_configAccessor = configAccessor;
		_scopedVideos = scopedVideos;
		_logger = logger;
	}

	/// <inheritdoc />
	public async Task<int> ResetScopedAsync(CancellationToken cancellationToken)
	{
		var enabled = _configAccessor().EnabledLibraryIds;
		if (enabled.Length == 0)
		{
			_logger.LogInformation("SponsorBlock reset requested but no libraries are enabled — nothing to do.");
			return 0;
		}

		var count = 0;
		foreach (var video in _scopedVideos(enabled))
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				await _writer.DeleteOwnedAsync(video.Id, cancellationToken).ConfigureAwait(false);
				await _store.DeleteAsync(video.Id, cancellationToken).ConfigureAwait(false);
				count++;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "SponsorBlock reset failed for item {ItemId}", video.Id);
			}
		}

		_logger.LogInformation("SponsorBlock reset complete: cleared {Count} items in scoped libraries.", count);
		return count;
	}

	private static IEnumerable<Video> EnumerateScoped(ILibraryManager libraryManager, Guid[] enabled)
	{
		var query = new InternalItemsQuery
		{
			AncestorIds = enabled,
			Recursive = true,
		};
		foreach (var item in libraryManager.GetItemList(query))
		{
			if (item is Video video)
			{
				yield return video;
			}
		}
	}
}
