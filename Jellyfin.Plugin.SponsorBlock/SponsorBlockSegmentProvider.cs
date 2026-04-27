using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaSegments;
using MediaBrowser.Model;
using MediaBrowser.Model.MediaSegments;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SponsorBlock;

/// <summary>
/// Provides media segments from SponsorBlock data.
/// </summary>
public class SponsorBlockSegmentProvider : IMediaSegmentProvider
{
	private readonly SponsorBlockApiClient _apiClient;
	private readonly ILibraryManager _libraryManager;
	private readonly ILogger<SponsorBlockSegmentProvider> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="SponsorBlockSegmentProvider"/> class.
	/// </summary>
	/// <param name="apiClient">The SponsorBlock API client.</param>
	/// <param name="libraryManager">The library manager for item lookups.</param>
	/// <param name="logger">The logger.</param>
	public SponsorBlockSegmentProvider(
		SponsorBlockApiClient apiClient,
		ILibraryManager libraryManager,
		ILogger<SponsorBlockSegmentProvider> logger)
	{
		_apiClient = apiClient;
		_libraryManager = libraryManager;
		_logger = logger;
	}

	/// <inheritdoc />
	public string Name => "SponsorBlock";

	/// <inheritdoc />
	public ValueTask<bool> Supports(BaseItem item)
	{
		var config = Plugin.Instance?.Configuration;
		if (config is null)
		{
			return ValueTask.FromResult(false);
		}

		var path = item.Path;
		if (string.IsNullOrEmpty(path))
		{
			return ValueTask.FromResult(false);
		}

		var filename = Path.GetFileName(path);
		var videoId = YouTubeIdExtractor.Extract(filename, config.FileMatchingMode, config.CustomRegexPattern);
		return ValueTask.FromResult(videoId is not null);
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<MediaSegmentDto>> GetMediaSegments(
		MediaSegmentGenerationRequest request,
		CancellationToken cancellationToken)
	{
		var config = Plugin.Instance?.Configuration;
		if (config is null)
		{
			return [];
		}

		var item = _libraryManager.GetItemById(request.ItemId);
		var path = item?.Path;
		if (string.IsNullOrEmpty(path))
		{
			return [];
		}

		var filename = Path.GetFileName(path);
		var videoId = YouTubeIdExtractor.Extract(filename, config.FileMatchingMode, config.CustomRegexPattern);
		if (videoId is null)
		{
			return [];
		}

		var enabledCategories = CategoryMapping.GetEnabledCategories(config.GetCategorySettings());
		if (enabledCategories.Count == 0)
		{
			return [];
		}

		_logger.LogDebug("Fetching SponsorBlock segments for {VideoId}", videoId);

		var apiSegments = await _apiClient
			.GetSegmentsAsync(videoId, enabledCategories, cancellationToken)
			.ConfigureAwait(false);

		var segments = MapSegments(apiSegments, request.ItemId);

		_logger.LogInformation("Found {Count} SponsorBlock segments for {VideoId}", segments.Count, videoId);

		return segments;
	}

	/// <summary>
	/// Maps SponsorBlock API segments to Jellyfin MediaSegmentDto objects.
	/// </summary>
	/// <param name="apiSegments">The API response segments.</param>
	/// <param name="itemId">The Jellyfin media item ID.</param>
	/// <returns>List of mapped segment DTOs.</returns>
	public static IReadOnlyList<MediaSegmentDto> MapSegments(
		IReadOnlyList<SponsorBlockSegment> apiSegments,
		Guid itemId) => SegmentMapper.Map(apiSegments, itemId);
}
