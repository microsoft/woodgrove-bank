using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace woodgrove_bank.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private TelemetryClient _telemetry;

    public IndexModel(ILogger<IndexModel> logger, TelemetryClient telemetry)
    {
        _logger = logger;
        _telemetry = telemetry;
    }

    public void OnGet()
    {
        _telemetry.TrackPageView("Index");
    }
}
