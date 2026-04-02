using Jellyfin.Database.Implementations.Enums;
using Xunit;

namespace Jellyfin.Plugin.SponsorBlock.Tests;

public class CategoryMappingTests
{
	[Theory]
	[InlineData("intro", MediaSegmentType.Intro)]
	[InlineData("outro", MediaSegmentType.Outro)]
	[InlineData("preview", MediaSegmentType.Preview)]
	[InlineData("sponsor", MediaSegmentType.Commercial)]
	[InlineData("selfpromo", MediaSegmentType.Commercial)]
	[InlineData("interaction", MediaSegmentType.Commercial)]
	[InlineData("filler", MediaSegmentType.Commercial)]
	[InlineData("music_offtopic", MediaSegmentType.Commercial)]
	public void Maps_SponsorBlockCategory_ToJellyfinType(string category, MediaSegmentType expected)
	{
		var result = CategoryMapping.ToSegmentType(category);
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("poi_highlight")]
	[InlineData("exclusive_access")]
	[InlineData("chapter")]
	[InlineData("unknown_category")]
	[InlineData("")]
	public void Returns_Null_ForUnsupportedCategories(string category)
	{
		var result = CategoryMapping.ToSegmentType(category);
		Assert.Null(result);
	}

	[Fact]
	public void GetEnabledCategories_ReturnsOnlyEnabled()
	{
		var enabled = new Dictionary<string, bool>
		{
			["sponsor"] = true,
			["selfpromo"] = false,
			["interaction"] = true,
			["intro"] = false,
			["outro"] = true,
			["preview"] = false,
			["filler"] = false,
			["music_offtopic"] = false,
		};

		var result = CategoryMapping.GetEnabledCategories(enabled);
		Assert.Equal(["sponsor", "interaction", "outro"], result);
	}
}
