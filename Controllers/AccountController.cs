using FinalProject.MVC.ViewModels.Auths;
using FinalProject.MVC.DataAccess;
using FinalProject.MVC.Extensions;
using FinalProject.MVC.Models;
using FinalProject.MVC.Services.Abstracts;
using FinalProject.MVC.Services.Implemets;
using FinalProject.MVC.Views.Account.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace FinalProject.MVC.Controllers;

public class AccountController(UserManager<User> _userManager, SignInManager<User> _signInManager, RoleManager<IdentityRole> _roleManager, IEmailService _service, AppDbContext _context, IMemoryCache _cache) : Controller
{
    private bool isAuthenticated => HttpContext.User.Identity?.IsAuthenticated ?? false;
    private readonly ILogger<AccountController> _logger;
    public IActionResult Register()
    {
        //if (isAuthenticated) return RedirectToAction("Index", "Home");
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Register(RegisterVM vm)
    {
        //if (isAuthenticated) return RedirectToAction("Index", "Home");
        if (!ModelState.IsValid)
            return View();
        User user = new User
        {
            Fullname = vm.Fullname,
            Email = vm.Email,
            UserName = vm.Username
        };
        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError("", err.Description);
            }
            return View();
        }
        //var roleResult = await _userManager.AddToRoleAsync(user, nameof(Roles.User));
        //if (!roleResult.Succeeded)
        //{
        //    foreach (var err in roleResult.Errors)
        //    {
        //        ModelState.AddModelError("", err.Description);
        //    }
        //    return View();
        //}
        Random r = new Random();
        int code = r.Next(1000, 9999);
        //await _context.UserCodes.AddAsync(new UserCode
        //{
        //    UserId = user.Id,
        //    Code = code
        //});
        //await _context.SaveChangesAsync();
        _cache.Set(user.Id, code, DateTimeOffset.Now.AddMinutes(30));
        _service.SendEmailConfirmation(user.Email, user.UserName, code.ToString());
        return Content("Email sent!");
    }

    //public async Task<IActionResult> Method()
    //{
    //    foreach (Roles item in Enum.GetValues(typeof(Roles)))
    //    {
    //        await _roleManager.CreateAsync(new IdentityRole(item.GetRole()));
    //    }
    //    return Ok();
    //}
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action("GoogleResponse", "Account");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            "Google",
            redirectUrl);
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
    public async Task<IActionResult> GoogleResponse(string returnUrl = null)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["ErrorMessage"] = "Error loading Google authentication information.";
            return RedirectToAction("Login");
        }

        // Try to sign in first
        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Email claim not received from Google.";
            return RedirectToAction("Login");
        }

        // Handle existing user
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return await HandleExistingUser(existingUser, info, returnUrl);
        }

        // Handle new user
        return await HandleNewUser(info, email, returnUrl);
    }

    private async Task<IActionResult> HandleExistingUser(User user, ExternalLoginInfo info, string returnUrl)
    {
        var result = await _userManager.AddLoginAsync(user, info);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }

        TempData["ErrorMessage"] = "This Google account is already linked to another user.";
        return RedirectToAction("Login");
    }

    private async Task<IActionResult> HandleNewUser(ExternalLoginInfo info, string email, string returnUrl)
    {
        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Failed to create account: " +
                string.Join(", ", createResult.Errors.Select(e => e.Description));
            return RedirectToAction("Register");
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Failed to link Google account: " +
                string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
            return RedirectToAction("Register");
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl ?? Url.Content("~/"));
    }
    public IActionResult Login()
    {
        //if (isAuthenticated) return RedirectToAction("Index", "Home");
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
    {
        //if (isAuthenticated) return RedirectToAction("Index", "Home");
        if (!ModelState.IsValid) return View();
        User? user = null;
        if (vm.UsernameOrEmail.Contains("@"))
            user = await _userManager.FindByEmailAsync(vm.UsernameOrEmail);
        else
            user = await _userManager.FindByNameAsync(vm.UsernameOrEmail);
        if (user is null)
        {
            ModelState.AddModelError("", "Username or password is wrong!");
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(user, vm.Password, vm.RememberMe, true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Wait until" + user.LockoutEnd!.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            if (result.IsNotAllowed)
            {
                ModelState.AddModelError("", "You must confirm your account");
            }
            return View();
        }

        //if (string.IsNullOrWhiteSpace(returnUrl))
        //{
        //    if (await _userManager.IsInRoleAsync(user,"Admin"))
        //    {
        //        return RedirectToAction("Index", new { Controller = "Dashboard", Area = "Admin" });
        //    }
        //    return RedirectToAction("Index", "Home");
        //}
        return LocalRedirect(returnUrl ?? "/");
    }
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
    public async Task<IActionResult> VerifyEmail(int code, string user)
    {
        var entity = await _userManager.FindByNameAsync(user);
        if (entity is null) return BadRequest();
        int? cacheCode = _cache.Get<int>(entity.Id);
        if (!cacheCode.HasValue || cacheCode != code)
            return BadRequest();
        entity.EmailConfirmed = true;
        await _userManager.UpdateAsync(entity);
        _cache.Remove(entity.Id);
        await _signInManager.SignInAsync(entity, true);
        return RedirectToAction("Index","Home");
        
    }
    public async Task<IActionResult> ResetPassword()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> ResetPassword(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return NotFound();
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        _service.SendEmailConfirmation(user.Email!, user.UserName!, token);
        return Content("Sent");
    }
    public async Task<IActionResult> NewPassword()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> NewPassword(string user,string token, string pass)
    {
        var entity = await _userManager.FindByNameAsync(user);
        if (entity == null) return NotFound();
        
        var result = await _userManager.ResetPasswordAsync(entity, token.Replace(' ','+'), pass);
        return Json(result.Succeeded);
    }
    [HttpGet]
    public IActionResult ResendEmailConfirmation()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> ResendEmailConfirmation(ResendEmailVM model )
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError("", "Email not found.");
            return View(model);
        }

        if (user.EmailConfirmed)
        {
            ModelState.AddModelError("", "Email is already confirmed.");
            return View(model);
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        _service.SendEmailConfirmation(user.Email, user.UserName, WebUtility.UrlEncode(token));

        ViewBag.Message = "Confirmation email sent!";
        return View(); // optionally redirect to success page
    }
}
