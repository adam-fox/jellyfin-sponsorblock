using Jellyfin.Plugin.SponsorBlock.Orchestration;
using Xunit;

namespace Jellyfin.Plugin.SponsorBlock.Tests.Orchestration;

public class MediaSegmentWriterProviderIdTests
{
	[Fact]
	public void ProviderId_Matches_JellyfinHashOfProviderName()
	{
		// Jellyfin's MediaSegmentManager.GetProviderId:
		//   name.ToLowerInvariant().GetMD5().ToString("N")
		// where GetMD5 = new Guid(MD5.HashData(Encoding.Unicode.GetBytes(s))).
		// For "SponsorBlock" this is the value Jellyfin will look up our segments under;
		// if this constant ever drifts, the public MediaSegments API silently hides our segments.
		Assert.Equal("4bc99c625103c30a9a5dbcaa3ace155c", IMediaSegmentWriter.ProviderId);
	}
}
