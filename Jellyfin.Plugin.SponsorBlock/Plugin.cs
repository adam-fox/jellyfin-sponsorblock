using System.Globalization;
using Jellyfin.Plugin.SponsorBlock.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.SponsorBlock;

/// <summary>
/// SponsorBlock plugin for Jellyfin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
	/// <summary>
	/// The plugin GUID.
	/// </summary>
	public const string PluginGuid = "c0e51a88-71a0-4f5c-82dc-81b8ae1a3e0f";

	/// <summary>
	/// Initializes a new instance of the <see cref="Plugin"/> class.
	/// </summary>
	/// <param name="applicationPaths">The application paths.</param>
	/// <param name="xmlSerializer">The XML serializer.</param>
	public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
		: base(applicationPaths, xmlSerializer)
	{
		Instance = this;
	}

	/// <summary>
	/// Gets the current plugin instance.
	/// </summary>
	public static Plugin? Instance { get; private set; }

	/// <inheritdoc />
	public override Guid Id => Guid.Parse(PluginGuid, CultureInfo.InvariantCulture);

	/// <inheritdoc />
	public override string Name => "SponsorBlock";

	/// <inheritdoc />
	public override string Description => "Skip sponsored segments in YouTube videos using SponsorBlock data.";

	/// <inheritdoc />
	public IEnumerable<PluginPageInfo> GetPages()
	{
		return
		[
			new PluginPageInfo
			{
				Name = Name,
				EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.configPage.html",
			},
		];
	}
}
