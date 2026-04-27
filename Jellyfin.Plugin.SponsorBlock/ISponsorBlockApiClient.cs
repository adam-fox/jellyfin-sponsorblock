namespace Jellyfin.Plugin.SponsorBlock;

/// <summary>
/// Fetches skip segments from the SponsorBlock public API.
/// </summary>
public interface ISponsorBlockApiClient
{
	/// <summary>
	/// Fetches skip segments for a YouTube video.
	/// </summary>
	/// <param name="videoId">The 11-character YouTube video ID.</param>
	/// <param name="categories">SponsorBlock category names to include.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>List of segments. Empty list if SponsorBlock has no data (HTTP 404 or 200 [] both map to empty).</returns>
	/// <exception cref="HttpRequestException">Network or transport error. Caller treats as transient.</exception>
	Task<IReadOnlyList<SponsorBlockSegment>> GetSegmentsAsync(
		string videoId,
		IReadOnlyList<string> categories,
		CancellationToken cancellationToken);
}
