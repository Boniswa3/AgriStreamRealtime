using AgriStreamRealtime.Data;
using AgriStreamRealtime.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace AgriStreamRealtime.Controllers
{
    public class AccountController : Controller
    {
        // Add your controller actions here
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }
        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user, string password)
        {
            ModelState.Remove(nameof(user.PasswordHash));
            ModelState.Remove(nameof(user.Id));

            if (!ModelState.IsValid)
                return View(user);

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                ModelState.AddModelError("", "Password must be at least 8 characters.");
                return View(user);
            }

            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username == user.Username);

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == user.Email);

            if (usernameExists || emailExists)
            {
                ModelState.AddModelError("", "Username or Email already exists.");
                return View(user);
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == user.RoleId);

            if (role == null)
            {
                ModelState.AddModelError("", "Please select a valid role.");
                return View(user);
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Registration successful. Please log in.";

            return RedirectToAction(nameof(Login));
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username,
            string password)
        {
            // Validate the input and ensure that both username and password are provided.
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("",
                    "Please enter your username and password.");
                return View();
            }

            // Retrieve the customer from the database based on the provided username and include the associated UserRole.
            var Role = await _context.Users
                .Include(c => c.Role)
                .FirstOrDefaultAsync(c => c.Username == username);

            if (Role == null || Role.Role == null)
            {
                ModelState.AddModelError("",
                    "Invalid username or password.");
                return View();
            }

            // Verify the provided password against the stored password hash using the PasswordHasher.
            var result = _passwordHasher.VerifyHashedPassword(
                Role,
                Role.PasswordHash,
                password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("",
                    "Invalid username or password.");
                return View();
            }

            // Create claims for the authenticated user, including their ID, username, and role.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,
                    Role.Id.ToString()),

                new Claim(ClaimTypes.Name, Role.Username),

                new Claim(ClaimTypes.Role,
                    Role.Role.RoleName)
            };

            // Create a ClaimsIdentity and ClaimsPrincipal for the authenticated user and sign them in using cookie authentication.
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            // Sign in the user and create an authentication cookie.
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            switch (Role.Role.RoleName)
            {
                case "Admin":
                    return RedirectToAction("Dashboard", "Admin");

                case "Agronomist":
                    return RedirectToAction("Dashboard", "Service");

                case "Farmer":
                    return RedirectToAction("Dashboard", "Home");

                default:
                    return RedirectToAction("Dashboard", "Home");
            }
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user and remove the authentication cookie.
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

