using System.Collections.Frozen;
using Jellyfin.Database.Implementations.Enums;

namespace Jellyfin.Plugin.SponsorBlock;

/// <summary>
/// Maps SponsorBlock categories to Jellyfin MediaSegmentType values.
/// </summary>
public static class CategoryMapping
{
	private static readonly FrozenDictionary<string, MediaSegmentType> Map = new Dictionary<string, MediaSegmentType>
	{
		["intro"] = MediaSegmentType.Intro,
		["outro"] = MediaSegmentType.Outro,
		["preview"] = MediaSegmentType.Preview,
		["sponsor"] = MediaSegmentType.Commercial,
		["selfpromo"] = MediaSegmentType.Commercial,
		["interaction"] = MediaSegmentType.Commercial,
		["filler"] = MediaSegmentType.Commercial,
		["music_offtopic"] = MediaSegmentType.Commercial,
	}.ToFrozenDictionary();

	/// <summary>
	/// Maps a SponsorBlock category to a Jellyfin segment type.
	/// </summary>
	/// <param name="category">The SponsorBlock category string.</param>
	/// <returns>The Jellyfin segment type, or null if the category is not supported.</returns>
	public static MediaSegmentType? ToSegmentType(string category)
	{
		return Map.TryGetValue(category, out var type) ? type : null;
	}

	/// <summary>
	/// Returns the list of SponsorBlock category names that are enabled.
	/// </summary>
	/// <param name="categorySettings">Dictionary of category name → enabled.</param>
	/// <returns>List of enabled category names.</returns>
	public static List<string> GetEnabledCategories(Dictionary<string, bool> categorySettings)
	{
		return categorySettings
			.Where(kv => kv.Value)
			.Select(kv => kv.Key)
			.ToList();
	}
}
