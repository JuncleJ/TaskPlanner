using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskPlanner.Pages;

public class RegisterModel : PageModel
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;

    public RegisterModel(
        UserManager<IdentityUser> users,
        SignInManager<IdentityUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    [BindProperty]
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [BindProperty]
    [Required, MinLength(8)]
    public string Password { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = new IdentityUser
        {
            UserName = Email.Trim(),
            Email = Email.Trim()
        };

        var result = await _users.CreateAsync(user, Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return Page();
        }

        await _signIn.SignInAsync(user, isPersistent: false);

        return RedirectToPage("/tasks");
    }
}