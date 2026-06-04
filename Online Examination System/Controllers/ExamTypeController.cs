using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Examination_System.Models;
using Online_Examination_System.Repositories;
using System.Security.Claims;

namespace Online_Examination_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExamTypeController : Controller
    {
        private readonly IExamTypeRepository _examTypeRepository;

        public ExamTypeController(IExamTypeRepository examTypeRepository)
        {
            _examTypeRepository = examTypeRepository;
        }

        public async Task<IActionResult> Index()
        {
            var examTypes = await _examTypeRepository.GetAllAsync();
            return View(examTypes);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                // Create
                return View(new ExamType { IsActive = true });
            }
            else
            {
                // Edit
                var examType = await _examTypeRepository.GetByIdAsync(id.Value);
                if (examType == null)
                    return NotFound();
                    
                return View(examType);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(ExamType model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _examTypeRepository.UpsertAsync(model, currentUserId);
                TempData["SuccessMessage"] = "Exam Type saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            await _examTypeRepository.ToggleStatusAsync(id);
            TempData["SuccessMessage"] = "Exam Type status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
