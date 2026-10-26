using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskPlanner.Pages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;

    public LogoutModel(SignInManager<IdentityUser> signIn)
    {
        _signIn = signIn;
    }

    public IActionResult OnGet()
    {
        return RedirectToPage("/tasks");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _signIn.SignOutAsync();
        return RedirectToPage("/login");
    }
}