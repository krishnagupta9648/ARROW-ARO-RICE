using ARROW_ARO_RICE.Models.ViewModels;
using ARROW_ARO_RICE.Services;
using Microsoft.AspNetCore.Mvc;

namespace ARROW_ARO_RICE.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Admin");
        }
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _authService.AuthenticateAsync(model.Email, model.Password);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        await _authService.SignInAsync(HttpContext, user, model.RememberMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Admin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync(HttpContext);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickLogin(string role, string? returnUrl)
    {
        var (email, password) = (role?.ToLower()) switch
        {
            "admin" => ("admin@arrowarorice.com", "Admin@2026#Rice"),
            "sales" => ("sales@arrowarorice.com", "Sales@2026#Rice"),
            "inventory" => ("inventory@arrowarorice.com", "Inventory@2026#Rice"),
            "accounts" => ("accounts@arrowarorice.com", "Accounts@2026#Rice"),
            _ => ("admin@arrowarorice.com", "Admin@2026#Rice")
        };

        var user = await _authService.AuthenticateAsync(email, password);
        if (user != null)
        {
            await _authService.SignInAsync(HttpContext, user, true);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Admin");
    }
}
