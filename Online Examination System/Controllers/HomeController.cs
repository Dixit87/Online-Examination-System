using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Examination_System.Models;

namespace Online_Examination_System.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly Online_Examination_System.Repositories.IUserRepository _userRepository;

        public HomeController(Online_Examination_System.Repositories.IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Dashboard", "StudentPanel");
            }

            try
            {
                var stats = await _userRepository.GetDashboardStatsAsync();
                return View(stats);
            }
            catch
            {
                // If sp_GetDashboardStats is not created yet, just pass empty model
                return View(new ViewModels.DashboardViewModel());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
