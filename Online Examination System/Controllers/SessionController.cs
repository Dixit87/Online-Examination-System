using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Examination_System.Models;
using Online_Examination_System.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Online_Examination_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SessionController : Controller
    {
        private readonly ISessionRepository _sessionRepository;

        public SessionController(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var sessions = await _sessionRepository.GetAllAsync();
            return View(sessions);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            if (id.HasValue && id > 0)
            {
                var session = await _sessionRepository.GetByIdAsync(id.Value);
                if (session == null) return NotFound();
                return View(session);
            }
            
            return View(new Session { Status = true });
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(Session model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                model.CreatedBy = currentUserId;

                await _sessionRepository.UpsertSessionAsync(model);
                
                TempData["SuccessMessage"] = "Session saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            await _sessionRepository.ToggleStatusAsync(id);
            TempData["SuccessMessage"] = "Session status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
