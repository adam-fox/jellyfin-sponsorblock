using Jellyfin.Plugin.SponsorBlock.Orchestration;
using Jellyfin.Plugin.SponsorBlock.Scoping;
using Jellyfin.Plugin.SponsorBlock.State;
using Jellyfin.Plugin.SponsorBlock.Triggers;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SponsorBlock;

/// <summary>
/// Registers plugin services with Jellyfin's DI container.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
	/// <inheritdoc />
	public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
	{
		serviceCollection.AddSingleton<SponsorBlockApiClient>();
		serviceCollection.AddSingleton<ISponsorBlockApiClient>(sp => sp.GetRequiredService<SponsorBlockApiClient>());

		serviceCollection.AddSingleton<ISponsorBlockStateStore>(sp =>
		{
			var paths = sp.GetRequiredService<IApplicationPaths>();
			var dir = Path.Combine(paths.DataPath, Plugin.PluginGuid);
			Directory.CreateDirectory(dir);
			var dbPath = Path.Combine(dir, "sponsorblock-state.db");
			var conn = new SqliteConnection($"Data Source={dbPath};Cache=Shared");
			conn.Open();
			return new SqliteSponsorBlockStateStore(conn);
		});

		serviceCollection.AddSingleton<ILibraryScopeService>(sp =>
			new LibraryScopeService(
				sp.GetRequiredService<ILibraryManager>(),
				() => Plugin.Instance!.Configuration));
		serviceCollection.AddSingleton<IMediaSegmentWriter, MediaSegmentWriter>();
		serviceCollection.AddSingleton<SponsorBlockOrchestrator>(sp =>
			new SponsorBlockOrchestrator(
				sp.GetRequiredService<ISponsorBlockApiClient>(),
				sp.GetRequiredService<ISponsorBlockStateStore>(),
				sp.GetRequiredService<ILibraryScopeService>(),
				sp.GetRequiredService<IMediaSegmentWriter>(),
				() => Plugin.Instance!.Configuration,
				TimeProvider.System,
				sp.GetRequiredService<ILogger<SponsorBlockOrchestrator>>()));

		serviceCollection.AddHostedService<ItemAddedHostedService>();
		serviceCollection.AddHostedService<PlaybackStartHostedService>();
		serviceCollection.AddHostedService<ItemRemovedHostedService>();
	}
}
