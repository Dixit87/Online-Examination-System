using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Online_Examination_System.Data;
using Online_Examination_System.Models;
using Online_Examination_System.Repositories;
using Online_Examination_System.ViewModels;

namespace Online_Examination_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _userRepository.GetUserByLoginAsync(model.UsernameOrEmail);

                if (user != null && user.IsActive)
                {
                    // Simple Hash verification (in real world, use BCrypt/Argon2)
                    string inputHash = HashPassword(model.Password);
                    if (user.PasswordHash == inputHash)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                            new Claim(ClaimTypes.Name, user.Name),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, user.RoleName)
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Invalid Username or Password");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while logging in: " + ex.Message + " " + ex.InnerException?.Message);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var student = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Contact = model.Contact,
                    Username = model.Username,
                    PasswordHash = HashPassword(model.Password),
                    Address = model.Address
                };

                int userId = await _userRepository.RegisterStudentAsync(student);

                if (userId > 0)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please login.";
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SeedAdmin()
        {
            var admin = new User
            {
                Name = "Super Admin",
                Email = "admin@exampro.com",
                Contact = "1234567890",
                Username = "admin",
                PasswordHash = HashPassword("Admin@123"),
                Address = "HQ"
            };

            // Using existing repository but updating role logic.
            // Since sp_StudentRegister assigns Student role, let's just use raw Dapper query here for seeding.
            var connection = _userRepository.GetType().GetField("_connectionFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_userRepository) as IDbConnectionFactory;
            using var conn = connection.CreateConnection();

            var roleId = await Dapper.SqlMapper.ExecuteScalarAsync<int>(conn, "SELECT RoleId FROM Roles WHERE RoleName = 'Admin'");

            var existingUser = await Dapper.SqlMapper.ExecuteScalarAsync<int>(conn, "SELECT COUNT(1) FROM Users WHERE Username = 'admin'");
            if (existingUser == 0)
            {
                await Dapper.SqlMapper.ExecuteAsync(conn, "INSERT INTO Users (RoleId, Name, Email, Contact, Username, PasswordHash, IsActive) VALUES (@RoleId, @Name, @Email, @Contact, @Username, @PasswordHash, 1)", new { RoleId = roleId, Name = admin.Name, Email = admin.Email, Contact = admin.Contact, Username = admin.Username, PasswordHash = admin.PasswordHash });
                return Content("Admin user 'admin' created successfully with password 'Admin@123'");
            }
            else
            {
                await Dapper.SqlMapper.ExecuteAsync(conn, "UPDATE Users SET PasswordHash = @PasswordHash, IsActive = 1 WHERE Username = 'admin'", new { PasswordHash = admin.PasswordHash });
                return Content("Admin user already existed. The password has been RESET to 'Admin@123'. You can now login.");
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userRepository.GetUserByEmailAsync(model.Email);
            if (user != null)
            {
                // Real world: generate token, send email. 
                // For this project: We just redirect to Reset Password and pass the email in TempData or query string to simulate.
                return RedirectToAction("ResetPassword", new { email = model.Email });
            }

            ModelState.AddModelError("", "No active user found with this email.");
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");
            
            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userRepository.GetUserByEmailAsync(model.Email);
            if (user != null)
            {
                string newHash = HashPassword(model.Password);
                await _userRepository.UpdatePasswordAsync(user.UserId, newHash);

                TempData["SuccessMessage"] = "Password has been reset successfully. Please login.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "No active user found with this email.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
