using Jellyfin.Plugin.SponsorBlock.Configuration;
using Jellyfin.Plugin.SponsorBlock.Scoping;
using Xunit;

namespace Jellyfin.Plugin.SponsorBlock.Tests.Scoping;

public class LibraryScopeServiceTests
{
	[Fact]
	public void IsInScope_ReturnsFalse_WhenAllowlistEmpty()
	{
		var libId = Guid.NewGuid();
		var config = new PluginConfiguration { EnabledLibraryIds = Array.Empty<Guid>() };
		var service = new LibraryScopeService(() => config, _ => new[] { libId });

		Assert.False(service.IsInScope(item: null!));
	}

	[Fact]
	public void IsInScope_ReturnsTrue_WhenItemAncestorIsAllowlisted()
	{
		var libId = Guid.NewGuid();
		var config = new PluginConfiguration { EnabledLibraryIds = new[] { libId } };
		var service = new LibraryScopeService(() => config, _ => new[] { Guid.NewGuid(), libId });

		Assert.True(service.IsInScope(item: null!));
	}

	[Fact]
	public void IsInScope_ReturnsFalse_WhenItemHasNoAllowlistedAncestor()
	{
		var libId = Guid.NewGuid();
		var config = new PluginConfiguration { EnabledLibraryIds = new[] { libId } };
		var service = new LibraryScopeService(() => config, _ => new[] { Guid.NewGuid(), Guid.NewGuid() });

		Assert.False(service.IsInScope(item: null!));
	}
}
