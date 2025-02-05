using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace woodgrove_bank
{
    public class HelpModel : PageModel
    {
        private TelemetryClient _telemetry;

        public HelpModel(TelemetryClient telemetry)
        {
            _telemetry = telemetry;
        }

        public void OnGet()
        {
            _telemetry.TrackPageView("Help");
        }
    }
}
