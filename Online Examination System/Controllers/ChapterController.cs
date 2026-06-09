using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Online_Examination_System.Models;
using Online_Examination_System.Repositories;
using Online_Examination_System.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Online_Examination_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ChapterController : Controller
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ISessionRepository _sessionRepository;

        public ChapterController(IChapterRepository chapterRepository, ISessionRepository sessionRepository)
        {
            _chapterRepository = chapterRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var chapters = await _chapterRepository.GetAllAsync();
            return View(chapters);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var model = new ChapterUpsertViewModel();
            
            // Populate Dropdown
            var sessions = await _sessionRepository.GetAllAsync();
            model.SessionList = sessions.Where(s => s.Status).Select(s => new SelectListItem
            {
                Text = s.Title,
                Value = s.Id.ToString()
            });

            if (id.HasValue && id > 0)
            {
                var chapter = await _chapterRepository.GetByIdAsync(id.Value);
                if (chapter == null) return NotFound();
                
                model.Id = chapter.Id;
                model.SessionId = chapter.SessionId;
                model.Title = chapter.Title;
                model.Description = chapter.Description;
                model.Status = chapter.Status;
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(ChapterUpsertViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                var chapter = new Chapter
                {
                    Id = model.Id,
                    SessionId = model.SessionId,
                    Title = model.Title,
                    Description = model.Description,
                    Status = model.Status,
                    CreatedBy = currentUserId
                };

                await _chapterRepository.UpsertChapterAsync(chapter);
                
                TempData["SuccessMessage"] = "Chapter saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            // Repopulate Dropdown on error
            var sessions = await _sessionRepository.GetAllAsync();
            model.SessionList = sessions.Where(s => s.Status).Select(s => new SelectListItem
            {
                Text = s.Title,
                Value = s.Id.ToString()
            });
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            await _chapterRepository.ToggleStatusAsync(id);
            TempData["SuccessMessage"] = "Chapter status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
