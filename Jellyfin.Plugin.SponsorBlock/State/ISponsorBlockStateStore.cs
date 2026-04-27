namespace Jellyfin.Plugin.SponsorBlock.State;

/// <summary>
/// Persistent per-item state store backing the SponsorBlock state machine.
/// </summary>
public interface ISponsorBlockStateStore
{
	/// <summary>Returns the row for an item, or null if absent.</summary>
	ValueTask<ItemStateRow?> GetAsync(Guid itemId, CancellationToken cancellationToken);

	/// <summary>Inserts or replaces the row for an item.</summary>
	ValueTask UpsertAsync(ItemStateRow row, CancellationToken cancellationToken);

	/// <summary>Deletes the row for an item if present.</summary>
	ValueTask DeleteAsync(Guid itemId, CancellationToken cancellationToken);

	/// <summary>
	/// Returns all rows in <see cref="ItemState.Pending"/> or <see cref="ItemState.HasData"/> state,
	/// for use by the daily refresh task. <see cref="ItemState.NoData"/> rows are excluded.
	/// </summary>
	IAsyncEnumerable<ItemStateRow> GetActiveAsync(CancellationToken cancellationToken);
}
