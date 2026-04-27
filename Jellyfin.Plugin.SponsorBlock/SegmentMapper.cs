using MediaBrowser.Model.MediaSegments;

namespace Jellyfin.Plugin.SponsorBlock;

/// <summary>
/// Converts SponsorBlock API segments into Jellyfin <see cref="MediaSegmentDto"/> instances.
/// </summary>
public static class SegmentMapper
{
	/// <summary>
	/// Maps SponsorBlock API segments to Jellyfin segment DTOs.
	/// Skips segments that aren't `skip` actions, have invalid timing, or use unsupported categories.
	/// </summary>
	/// <param name="apiSegments">Segments returned by the SponsorBlock API.</param>
	/// <param name="itemId">The Jellyfin item id the segments are for.</param>
	/// <returns>Mapped DTOs in input order.</returns>
	public static IReadOnlyList<MediaSegmentDto> Map(
		IReadOnlyList<SponsorBlockSegment> apiSegments,
		Guid itemId)
	{
		var result = new List<MediaSegmentDto>();
		foreach (var seg in apiSegments)
		{
			if (seg.ActionType != "skip" || seg.Segment.Length < 2)
			{
				continue;
			}

			var segmentType = CategoryMapping.ToSegmentType(seg.Category);
			if (segmentType is null)
			{
				continue;
			}

			result.Add(new MediaSegmentDto
			{
				Id = Guid.NewGuid(),
				ItemId = itemId,
				Type = segmentType.Value,
				StartTicks = (long)(seg.Segment[0] * TimeSpan.TicksPerSecond),
				EndTicks = (long)(seg.Segment[1] * TimeSpan.TicksPerSecond),
			});
		}

		return result;
	}
}
