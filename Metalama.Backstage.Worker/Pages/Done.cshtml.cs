// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Welcome;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metalama.Backstage.Pages;

#pragma warning disable SA1649

public class DonePageModel : PageModel
{
    private readonly WelcomeService _welcomeService;

    public DonePageModel( WelcomeService welcomeService )
    {
        this._welcomeService = welcomeService;
    }

    public IActionResult OnGet()
    {
        var url = this._welcomeService.GetWelcomePageUrlAndRemember();
        
        return this.Redirect( url );
    }
}