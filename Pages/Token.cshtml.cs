using System.Text.Json;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace woodgrove_bank.Pages;

[Authorize]
public class TokenModel : PageModel
{
  private readonly ILogger<TokenModel> _logger;
  private TelemetryClient _telemetry;

  public TokenModel(ILogger<TokenModel> logger, TelemetryClient telemetry)
  {
    _logger = logger;
    _telemetry = telemetry;
  }

  public void OnGet()
  {
    _telemetry.TrackPageView("Token");
  }
}

