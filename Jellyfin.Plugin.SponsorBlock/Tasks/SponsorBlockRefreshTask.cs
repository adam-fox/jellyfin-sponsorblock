using Jellyfin.Plugin.SponsorBlock.Configuration;
using Jellyfin.Plugin.SponsorBlock.Orchestration;
using Jellyfin.Plugin.SponsorBlock.State;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SponsorBlock.Tasks;

/// <summary>
/// Daily refresh task: walks all Pending + HasData rows, runs the orchestrator with DailyScan reason.
/// Drops orphan rows whose item no longer exists in the library.
/// </summary>
public sealed class SponsorBlockRefreshTask : IScheduledTask
{
	private readonly ISponsorBlockStateStore _store;
	private readonly ILibraryManager _libraryManager;
	private readonly SponsorBlockOrchestrator _orchestrator;
	private readonly IMediaSegmentWriter _writer;
	private readonly ILogger<SponsorBlockRefreshTask> _logger;

	/// <summary>Initializes the scheduled task.</summary>
	/// <param name="store">Per-item state store.</param>
	/// <param name="libraryManager">Jellyfin library manager.</param>
	/// <param name="orchestrator">Orchestrator instance.</param>
	/// <param name="writer">Wrapper around Jellyfin media segment manager.</param>
	/// <param name="logger">Logger.</param>
	public SponsorBlockRefreshTask(
		ISponsorBlockStateStore store,
		ILibraryManager libraryManager,
		SponsorBlockOrchestrator orchestrator,
		IMediaSegmentWriter writer,
		ILogger<SponsorBlockRefreshTask> logger)
	{
		_store = store;
		_libraryManager = libraryManager;
		_orchestrator = orchestrator;
		_writer = writer;
		_logger = logger;
	}

	/// <inheritdoc />
	public string Name => "SponsorBlock daily refresh";

	/// <inheritdoc />
	public string Key => "SponsorBlockRefresh";

	/// <inheritdoc />
	public string Description => "Refreshes SponsorBlock segments for tracked items and runs the 48-hour sanity check on items with no data.";

	/// <inheritdoc />
	public string Category => "SponsorBlock";

	/// <inheritdoc />
	public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
	{
		var hour = Plugin.Instance?.Configuration.DailyScanHour ?? 6;
		return new[]
		{
			new TaskTriggerInfo
			{
				Type = TaskTriggerInfoType.DailyTrigger,
				TimeOfDayTicks = TimeSpan.FromHours(hour).Ticks,
			},
		};
	}

	/// <inheritdoc />
	public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
	{
		var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();

		var rows = new List<ItemStateRow>();
		await foreach (var row in _store.GetActiveAsync(cancellationToken).ConfigureAwait(false))
		{
			rows.Add(row);
		}

		if (rows.Count == 0)
		{
			progress.Report(100);
			return;
		}

		for (var i = 0; i < rows.Count; i++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var row = rows[i];

			var item = _libraryManager.GetItemById(row.ItemId);
			if (item is null)
			{
				_logger.LogInformation("Dropping orphan SponsorBlock state for missing item {ItemId}", row.ItemId);
				try
				{
					await _writer.DeleteOwnedAsync(row.ItemId, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to delete owned segments for orphan {ItemId}", row.ItemId);
				}

				await _store.DeleteAsync(row.ItemId, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				try
				{
					await _orchestrator.ProcessAsync(item, ProcessReason.DailyScan, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Daily refresh failed for item {ItemId}", row.ItemId);
				}
			}

			progress.Report(100.0 * (i + 1) / rows.Count);

			if (config.RequestDelayMilliseconds > 0)
			{
				await Task.Delay(config.RequestDelayMilliseconds, cancellationToken).ConfigureAwait(false);
			}
		}
	}
}
