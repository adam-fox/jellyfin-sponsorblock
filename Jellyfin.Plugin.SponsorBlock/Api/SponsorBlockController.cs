using Jellyfin.Plugin.SponsorBlock.Reset;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.SponsorBlock.Api;

/// <summary>
/// Admin-only HTTP endpoints for the SponsorBlock plugin.
/// </summary>
[ApiController]
[Authorize(Policy = "RequiresElevation")]
[Route("Plugins/SponsorBlock")]
public sealed class SponsorBlockController : ControllerBase
{
	private readonly IResetService _resetService;

	/// <summary>Initializes the controller.</summary>
	/// <param name="resetService">Reset orchestrator.</param>
	public SponsorBlockController(IResetService resetService)
	{
		_resetService = resetService;
	}

	/// <summary>
	/// Wipes SponsorBlock state and owned segments for every item in the configured library scope.
	/// </summary>
	/// <returns>The number of items processed.</returns>
	[HttpPost("Reset")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<ResetResponse>> ResetAsync(CancellationToken cancellationToken)
	{
		var count = await _resetService.ResetScopedAsync(cancellationToken).ConfigureAwait(false);
		return new ResetResponse(count);
	}
}

/// <summary>Reset endpoint response payload.</summary>
/// <param name="ItemsProcessed">Count of items wiped.</param>
public sealed record ResetResponse(int ItemsProcessed);
