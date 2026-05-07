using FoodOutlet.AppCode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodOutlet.Controllers
{
    public class AccountController : Controller
    {
        private readonly Staff _staff;

        public AccountController(Staff staff)
        {
            _staff = staff;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["Error"] = "Email and password are required.";
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            var staff = _staff.LoginStaff(email.Trim(), password);

            if (staff == null)
            {
                var blocked = _staff.IsLoginBlockedByResignation(email.Trim(), password);
                ViewData["Error"] = blocked
                    ? "Your resignation is pending or approved — you cannot log in. If your resignation was rejected, try again or contact an administrator."
                    : "Invalid email or password.";
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim("StaffId",   staff.id.ToString()),
                new Claim("Name",      staff.name),
                new Claim("Email",     staff.email),
                new Claim("RoleName",  staff.role_name),
                new Claim("Photo",     staff.photo ?? ""),
                new Claim(ClaimTypes.Name, staff.email),
                new Claim(ClaimTypes.Role, staff.role_name),
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
