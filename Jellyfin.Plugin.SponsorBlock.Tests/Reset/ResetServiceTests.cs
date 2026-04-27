using Jellyfin.Plugin.SponsorBlock.Configuration;
using Jellyfin.Plugin.SponsorBlock.Orchestration;
using Jellyfin.Plugin.SponsorBlock.Reset;
using Jellyfin.Plugin.SponsorBlock.State;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Jellyfin.Plugin.SponsorBlock.Tests.Reset;

public class ResetServiceTests
{
	private readonly IMediaSegmentWriter _writer = Substitute.For<IMediaSegmentWriter>();
	private readonly ISponsorBlockStateStore _store = Substitute.For<ISponsorBlockStateStore>();

	private sealed class TestVideo : Video
	{
	}

	private static Video MakeVideo() => new TestVideo { Id = Guid.NewGuid() };

	private ResetService Make(PluginConfiguration config, params Video[] scoped) => new(
		_writer, _store,
		() => config,
		_ => scoped,
		NullLogger<ResetService>.Instance);

	[Fact]
	public async Task ResetScopedAsync_ReturnsZero_WhenNoLibrariesEnabled()
	{
		var service = Make(new PluginConfiguration { EnabledLibraryIds = Array.Empty<Guid>() }, MakeVideo());

		var count = await service.ResetScopedAsync(CancellationToken.None);

		Assert.Equal(0, count);
		await _writer.DidNotReceiveWithAnyArgs().DeleteOwnedAsync(default, default);
		await _store.DidNotReceiveWithAnyArgs().DeleteAsync(default, default);
	}

	[Fact]
	public async Task ResetScopedAsync_DeletesSegmentsAndStateForEachScopedVideo()
	{
		var v1 = MakeVideo();
		var v2 = MakeVideo();
		var config = new PluginConfiguration { EnabledLibraryIds = new[] { Guid.NewGuid() } };
		var service = Make(config, v1, v2);

		var count = await service.ResetScopedAsync(CancellationToken.None);

		Assert.Equal(2, count);
		await _writer.Received(1).DeleteOwnedAsync(v1.Id, Arg.Any<CancellationToken>());
		await _writer.Received(1).DeleteOwnedAsync(v2.Id, Arg.Any<CancellationToken>());
		await _store.Received(1).DeleteAsync(v1.Id, Arg.Any<CancellationToken>());
		await _store.Received(1).DeleteAsync(v2.Id, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ResetScopedAsync_ContinuesPastFailures()
	{
		var v1 = MakeVideo();
		var v2 = MakeVideo();
		var config = new PluginConfiguration { EnabledLibraryIds = new[] { Guid.NewGuid() } };
		_writer.DeleteOwnedAsync(v1.Id, Arg.Any<CancellationToken>())
			.Returns(Task.FromException(new InvalidOperationException("boom")));

		var service = Make(config, v1, v2);

		var count = await service.ResetScopedAsync(CancellationToken.None);

		Assert.Equal(1, count);
		await _store.DidNotReceive().DeleteAsync(v1.Id, Arg.Any<CancellationToken>());
		await _writer.Received(1).DeleteOwnedAsync(v2.Id, Arg.Any<CancellationToken>());
		await _store.Received(1).DeleteAsync(v2.Id, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ResetScopedAsync_RespectsCancellation()
	{
		var v1 = MakeVideo();
		var v2 = MakeVideo();
		var config = new PluginConfiguration { EnabledLibraryIds = new[] { Guid.NewGuid() } };
		using var cts = new CancellationTokenSource();
		cts.Cancel();
		var service = Make(config, v1, v2);

		await Assert.ThrowsAsync<OperationCanceledException>(() => service.ResetScopedAsync(cts.Token));
		await _writer.DidNotReceiveWithAnyArgs().DeleteOwnedAsync(default, default);
	}
}
