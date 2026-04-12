using Concert.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Concert.Controllers;

public class AccountController : Controller
{
    private readonly ConcertContext _context;
    private readonly Services.EmailService _emailService;

    public AccountController(ConcertContext context, Services.EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var existingUser = await _context.Customers.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Користувач з такою поштою вже існує!");
                return View(model);
            }

            bool isFirstUser = !await _context.Customers.AnyAsync();

            var user = new Customer
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                IsAdmin = isFirstUser,
                BirthDate = model.BirthDate,
                LoyaltyDiscount = 0m
            };

            _context.Customers.Add(user);
            await _context.SaveChangesAsync();

            await Authenticate(user);
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }

    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Customers.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
            if (user != null)
            {
                await Authenticate(user);
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Невірний логін або пароль");
        }
        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => Content("У вас немає прав для доступу до цієї сторінки.");

    private async Task Authenticate(Customer user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("UserId", user.CustomerId.ToString())
        };

        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }

    // --- ВІДНОВЛЕННЯ ПАРОЛЯ ---
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Customers.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null)
            {
                var token = Guid.NewGuid().ToString();
                user.ResetPasswordToken = token;
                user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
                await _context.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Account", new { token = token, email = user.Email }, Request.Scheme);
                string message = $"<h3>Скидання пароля</h3><p>Щоб скинути пароль, перейдіть за посиланням: <a href='{resetLink}'>Скинути пароль</a></p><p>Посилання дійсне 1 годину.</p>";
                await _emailService.SendEmailAsync(user.Email!, "Відновлення пароля", message);
            }
            ViewBag.Message = "Якщо така пошта існує в системі, на неї відправлено лист.";
            return View("ForgotPasswordConfirmation");
        }
        return View(model);
    }

    public IActionResult ResetPassword(string token, string email)
    {
        if (token == null || email == null) return Content("Некоректне посилання.");
        return View(new ResetPasswordViewModel { Token = token, Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _context.Customers.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null || user.ResetPasswordToken != model.Token || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
        {
            ModelState.AddModelError("", "Посилання недійсне або його час дії минув.");
            return View(model);
        }

        user.Password = model.Password;
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiry = null;
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    // --- ЗМІНА ПАРОЛЯ В КАБІНЕТІ ---
    [Authorize]
    public IActionResult ChangePassword() => View();

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (userIdClaim == null) return RedirectToAction("Login");

        var user = await _context.Customers.FindAsync(int.Parse(userIdClaim));
        if (user == null) return NotFound();

        if (user.Password != model.OldPassword)
        {
            ModelState.AddModelError("OldPassword", "Поточний пароль вказано невірно.");
            return View(model);
        }

        user.Password = model.NewPassword;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Home");
    }
}