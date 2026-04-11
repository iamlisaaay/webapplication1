using Concert.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Concert.Controllers;

public class AccountController : Controller
{
    private readonly ConcertContext _context;

    public AccountController(ConcertContext context)
    {
        _context = context;
    }

    // Сторінка Реєстрації
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

            // ФІШКА: Якщо в базі ще немає користувачів, перший автоматично стає Адміном!
            bool isFirstUser = !await _context.Customers.AnyAsync();

            var user = new Customer
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                IsAdmin = isFirstUser,

                // --- НОВІ ПОЛЯ ---
                BirthDate = model.BirthDate, // Беремо дату з форми
                LoyaltyDiscount = 0          // Новим користувачам ставимо знижку 0
                // -----------------
            };

            _context.Customers.Add(user);
            await _context.SaveChangesAsync();

            await Authenticate(user); // Одразу логінимо
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }

    // Сторінка Входу
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

    // Вихід з акаунту
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => Content("У вас немає прав для доступу до цієї сторінки. Зверніться до адміністратора.");

    // Допоміжний метод: Видає користувачу віртуальний "квиток" (Cookie)
    private async Task Authenticate(Customer user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserId", user.CustomerId.ToString())
        };

        // Якщо це ти (Адмін), додаємо тобі спеціальну роль
        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }
}