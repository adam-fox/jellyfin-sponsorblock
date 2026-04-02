using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.SponsorBlock.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
	/// <summary>
	/// Gets or sets the file matching mode.
	/// </summary>
	public FileMatchingMode FileMatchingMode { get; set; } = FileMatchingMode.YouTubeIdAsFilename;

	/// <summary>
	/// Gets or sets the custom regex pattern for extracting YouTube IDs.
	/// </summary>
	public string CustomRegexPattern { get; set; } = @"\[([a-zA-Z0-9_-]{11})\]";

	/// <summary>
	/// Gets or sets a value indicating whether to skip sponsor segments.
	/// </summary>
	public bool Sponsor { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether to skip self-promotion segments.
	/// </summary>
	public bool SelfPromo { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether to skip interaction segments.
	/// </summary>
	public bool Interaction { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to skip intro segments.
	/// </summary>
	public bool Intro { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to skip outro segments.
	/// </summary>
	public bool Outro { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to skip preview segments.
	/// </summary>
	public bool Preview { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to skip filler segments.
	/// </summary>
	public bool Filler { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to skip off-topic music segments.
	/// </summary>
	public bool MusicOfftopic { get; set; }

	/// <summary>
	/// Returns a dictionary of category name to enabled for use with the API.
	/// </summary>
	/// <returns>Category settings dictionary.</returns>
	public Dictionary<string, bool> GetCategorySettings()
	{
		return new Dictionary<string, bool>
		{
			["sponsor"] = Sponsor,
			["selfpromo"] = SelfPromo,
			["interaction"] = Interaction,
			["intro"] = Intro,
			["outro"] = Outro,
			["preview"] = Preview,
			["filler"] = Filler,
			["music_offtopic"] = MusicOfftopic,
		};
	}
}
