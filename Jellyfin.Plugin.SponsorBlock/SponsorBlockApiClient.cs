using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SponsorBlock;

/// <summary>
/// HTTP client for the SponsorBlock API.
/// </summary>
public class SponsorBlockApiClient : ISponsorBlockApiClient
{
	private const string BaseUrl = "https://sponsor.ajay.app";
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<SponsorBlockApiClient> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="SponsorBlockApiClient"/> class.
	/// </summary>
	/// <param name="httpClientFactory">The HTTP client factory.</param>
	/// <param name="logger">The logger.</param>
	public SponsorBlockApiClient(IHttpClientFactory httpClientFactory, ILogger<SponsorBlockApiClient> logger)
	{
		_httpClientFactory = httpClientFactory;
		_logger = logger;
	}

	/// <summary>
	/// Fetches skip segments for a YouTube video.
	/// </summary>
	/// <param name="videoId">The YouTube video ID.</param>
	/// <param name="categories">List of SponsorBlock categories to fetch.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>List of segments, or empty list if none found.</returns>
	public async Task<IReadOnlyList<SponsorBlockSegment>> GetSegmentsAsync(
		string videoId,
		IReadOnlyList<string> categories,
		CancellationToken cancellationToken)
	{
		var categoriesJson = JsonSerializer.Serialize(categories);
		var url = $"/api/skipSegments?videoID={Uri.EscapeDataString(videoId)}&categories={Uri.EscapeDataString(categoriesJson)}&actionTypes={Uri.EscapeDataString("[\"skip\"]")}";

		var httpClient = _httpClientFactory.CreateClient("SponsorBlock");
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JellyfinSponsorBlock/1.0");

		var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			_logger.LogDebug("No SponsorBlock segments found for {VideoId}", videoId);
			return [];
		}

		response.EnsureSuccessStatusCode();

		var segments = await response.Content
			.ReadFromJsonAsync<List<SponsorBlockSegment>>(cancellationToken)
			.ConfigureAwait(false);

		return segments ?? [];
	}
}

/// <summary>
/// A segment returned by the SponsorBlock API.
/// </summary>
public sealed class SponsorBlockSegment
{
	/// <summary>
	/// Gets or sets the segment category.
	/// </summary>
	[JsonPropertyName("category")]
	public required string Category { get; set; }

	/// <summary>
	/// Gets or sets the action type.
	/// </summary>
	[JsonPropertyName("actionType")]
	public required string ActionType { get; set; }

	/// <summary>
	/// Gets or sets the segment start and end times in seconds.
	/// </summary>
	[JsonPropertyName("segment")]
	public required double[] Segment { get; set; }

	/// <summary>
	/// Gets or sets the segment UUID.
	/// </summary>
	[JsonPropertyName("UUID")]
	public required string UUID { get; set; }
}
