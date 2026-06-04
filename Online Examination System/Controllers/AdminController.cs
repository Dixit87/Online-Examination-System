using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Examination_System.Models;
using Online_Examination_System.Repositories;
using Online_Examination_System.ViewModels;
using Online_Examination_System.Helpers;
using System.Security.Claims;

namespace Online_Examination_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AdminController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Users(string searchTerm)
        {
            var users = await _userRepository.GetAllUsersAsync();
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u => u.Name.ToLower().Contains(searchTerm) || 
                                         u.Email.ToLower().Contains(searchTerm) || 
                                         u.Username.ToLower().Contains(searchTerm));
            }
            
            ViewBag.SearchTerm = searchTerm;
            return View(users);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            return View(new UserAddViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(UserAddViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Contact = model.Contact,
                    Username = model.Username,
                    PasswordHash = PasswordHelper.HashPassword(model.Password),
                    Address = model.Address,
                    IsActive = model.IsActive
                };

                await _userRepository.AddUserAsync(user, model.RoleName, currentUserId);
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserEditViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Contact = user.Contact,
                Username = user.Username,
                Address = user.Address,
                IsActive = user.IsActive
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                var user = new User
                {
                    UserId = model.UserId,
                    Name = model.Name,
                    Email = model.Email,
                    Contact = model.Contact,
                    Username = model.Username,
                    Address = model.Address,
                    IsActive = model.IsActive
                };

                await _userRepository.UpdateUserAsync(user, currentUserId);
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            await _userRepository.ToggleStatusAsync(id);
            TempData["SuccessMessage"] = "User status updated successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var stats = await _userRepository.GetDashboardStatsAsync();
            return View(stats);
        }
    }
}
