namespace Jellyfin.Plugin.SponsorBlock.Configuration;

/// <summary>
/// How to extract YouTube video IDs from filenames.
/// </summary>
public enum FileMatchingMode
{
	/// <summary>
	/// Filename without extension is the YouTube ID (e.g., 4pG8_bWpmaE.mp4).
	/// </summary>
	YouTubeIdAsFilename = 0,

	/// <summary>
	/// Use a custom regex with one capture group.
	/// </summary>
	CustomRegex = 1,
}
