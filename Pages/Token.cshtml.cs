using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace woodgrove_bank.Pages;

  [Authorize]
public class TokenModel : PageModel
{
    private readonly ILogger<TokenModel> _logger;

    public TokenModel(ILogger<TokenModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}

