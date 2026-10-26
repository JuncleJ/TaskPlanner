using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskPlanner.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;

    public LoginModel(SignInManager<IdentityUser> signIn)
    {
        _signIn = signIn;
    }

    [BindProperty]
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [BindProperty]
    [Required]
    public string Password { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
    {
    if (!ModelState.IsValid)
    {
        Console.WriteLine("LOGIN: ModelState invalid");
        return Page();
    }

    var result = await _signIn.PasswordSignInAsync(
        Email.Trim(),
        Password,
        isPersistent: false,
        lockoutOnFailure: true
    );

    Console.WriteLine($"LOGIN: success={result.Succeeded}");
    Console.WriteLine($"LOGIN: locked={result.IsLockedOut}");
    Console.WriteLine($"LOGIN: notAllowed={result.IsNotAllowed}");

    if (result.Succeeded)
    {
        Console.WriteLine("LOGIN: Redirect to Tasks");
        return RedirectToPage("/tasks");
    }

    ModelState.AddModelError("", "Ошибка авторизации");

    return Page();
    }
}