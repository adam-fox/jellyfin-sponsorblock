using Jellyfin.Plugin.SponsorBlock.Orchestration;
using Jellyfin.Plugin.SponsorBlock.State;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SponsorBlock.Triggers;

/// <summary>
/// Drops the per-item SQLite row and owned segments when a library item is removed.
/// </summary>
public sealed class ItemRemovedHostedService : IHostedService
{
	private readonly ILibraryManager _libraryManager;
	private readonly ISponsorBlockStateStore _store;
	private readonly IMediaSegmentWriter _writer;
	private readonly ILogger<ItemRemovedHostedService> _logger;

	/// <summary>Initializes the hosted service.</summary>
	/// <param name="libraryManager">Jellyfin library manager.</param>
	/// <param name="store">Per-item state store.</param>
	/// <param name="writer">Wrapper around Jellyfin media segment manager.</param>
	/// <param name="logger">Logger.</param>
	public ItemRemovedHostedService(
		ILibraryManager libraryManager,
		ISponsorBlockStateStore store,
		IMediaSegmentWriter writer,
		ILogger<ItemRemovedHostedService> logger)
	{
		_libraryManager = libraryManager;
		_store = store;
		_writer = writer;
		_logger = logger;
	}

	/// <inheritdoc />
	public Task StartAsync(CancellationToken cancellationToken)
	{
		_libraryManager.ItemRemoved += OnItemRemoved;
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task StopAsync(CancellationToken cancellationToken)
	{
		_libraryManager.ItemRemoved -= OnItemRemoved;
		return Task.CompletedTask;
	}

	private void OnItemRemoved(object? sender, ItemChangeEventArgs e)
	{
		var itemId = e.Item.Id;
		_ = Task.Run(async () =>
		{
			try
			{
				await _writer.DeleteOwnedAsync(itemId, CancellationToken.None).ConfigureAwait(false);
				await _store.DeleteAsync(itemId, CancellationToken.None).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "SponsorBlock cleanup failed for removed item {ItemId}", itemId);
			}
		});
	}
}
