using Jellyfin.Plugin.SponsorBlock.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.SponsorBlock.Scoping;

/// <summary>
/// Default implementation of <see cref="ILibraryScopeService"/>.
/// </summary>
public sealed class LibraryScopeService : ILibraryScopeService
{
	private readonly Func<PluginConfiguration> _configAccessor;
	private readonly Func<BaseItem, IEnumerable<Guid>> _ancestorIds;

	/// <summary>
	/// Production constructor — pulls collection-folder ids via <see cref="ILibraryManager.GetCollectionFolders(BaseItem)"/>.
	/// </summary>
	/// <param name="libraryManager">Jellyfin library manager.</param>
	/// <param name="configAccessor">Returns the current plugin configuration.</param>
	public LibraryScopeService(ILibraryManager libraryManager, Func<PluginConfiguration> configAccessor)
		: this(configAccessor, item => GetCollectionFolderIds(item, libraryManager))
	{
	}

	/// <summary>
	/// Test constructor — accepts an injected ancestor-id source so tests don't need a real BaseItem.
	/// </summary>
	/// <param name="configAccessor">Returns the current plugin configuration.</param>
	/// <param name="ancestorIds">Resolves an item to the GUIDs of its containing collection folders.</param>
	internal LibraryScopeService(
		Func<PluginConfiguration> configAccessor,
		Func<BaseItem, IEnumerable<Guid>> ancestorIds)
	{
		_configAccessor = configAccessor;
		_ancestorIds = ancestorIds;
	}

	/// <inheritdoc />
	public bool IsInScope(BaseItem item)
	{
		var allow = _configAccessor().EnabledLibraryIds;
		if (allow.Length == 0)
		{
			return false;
		}

		var allowSet = new HashSet<Guid>(allow);
		foreach (var id in _ancestorIds(item))
		{
			if (allowSet.Contains(id))
			{
				return true;
			}
		}

		return false;
	}

	private static IEnumerable<Guid> GetCollectionFolderIds(BaseItem item, ILibraryManager libraryManager)
	{
		foreach (var folder in libraryManager.GetCollectionFolders(item))
		{
			yield return folder.Id;
		}
	}
}
