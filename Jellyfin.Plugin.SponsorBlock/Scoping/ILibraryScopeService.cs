using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.SponsorBlock.Scoping;

/// <summary>
/// Decides whether a library item is in scope for SponsorBlock processing.
/// </summary>
public interface ILibraryScopeService
{
	/// <summary>
	/// Returns true iff the item belongs to a CollectionFolder whose id is in the configured
	/// allowlist. Empty allowlist → always false (safe default for fresh installs).
	/// </summary>
	/// <param name="item">The library item to check.</param>
	bool IsInScope(BaseItem item);
}
