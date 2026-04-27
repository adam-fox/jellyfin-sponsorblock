using Jellyfin.Database.Implementations.Enums;
using Xunit;

namespace Jellyfin.Plugin.SponsorBlock.Tests;

public class SegmentMapperTests
{
	[Fact]
	public void Map_ConvertsApiResponseToDto()
	{
		var apiSegments = new List<SponsorBlockSegment>
		{
			new()
			{
				Category = "sponsor",
				ActionType = "skip",
				Segment = [10.5, 30.0],
				UUID = "abc123",
			},
			new()
			{
				Category = "intro",
				ActionType = "skip",
				Segment = [0.0, 5.0],
				UUID = "def456",
			},
		};

		var itemId = Guid.NewGuid();
		var result = SegmentMapper.Map(apiSegments, itemId);

		Assert.Equal(2, result.Count);

		Assert.Equal(itemId, result[0].ItemId);
		Assert.Equal(MediaSegmentType.Commercial, result[0].Type);
		Assert.Equal((long)(10.5 * TimeSpan.TicksPerSecond), result[0].StartTicks);
		Assert.Equal((long)(30.0 * TimeSpan.TicksPerSecond), result[0].EndTicks);

		Assert.Equal(MediaSegmentType.Intro, result[1].Type);
		Assert.Equal(0L, result[1].StartTicks);
		Assert.Equal((long)(5.0 * TimeSpan.TicksPerSecond), result[1].EndTicks);
	}

	[Fact]
	public void Map_SkipsUnknownCategories()
	{
		var apiSegments = new List<SponsorBlockSegment>
		{
			new()
			{
				Category = "poi_highlight",
				ActionType = "poi",
				Segment = [18.0, 18.0],
				UUID = "ghi789",
			},
		};

		var result = SegmentMapper.Map(apiSegments, Guid.NewGuid());
		Assert.Empty(result);
	}

	[Fact]
	public void Map_SkipsNonSkipActionTypes()
	{
		var apiSegments = new List<SponsorBlockSegment>
		{
			new()
			{
				Category = "sponsor",
				ActionType = "mute",
				Segment = [10.0, 20.0],
				UUID = "jkl012",
			},
		};

		var result = SegmentMapper.Map(apiSegments, Guid.NewGuid());
		Assert.Empty(result);
	}
}
