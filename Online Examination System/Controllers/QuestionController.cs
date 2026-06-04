using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Examination_System.Models;
using Online_Examination_System.Repositories;
using Online_Examination_System.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Online_Examination_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QuestionController : Controller
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionController(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var questions = await _questionRepository.GetAllAsync();
            return View(questions);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var model = new QuestionUpsertViewModel();

            if (id.HasValue && id > 0)
            {
                var question = await _questionRepository.GetByIdAsync(id.Value);
                if (question == null) return NotFound();

                model.QuestionId = question.QuestionId;
                model.QuestionText = question.QuestionText;
                model.IsActive = question.IsActive;
                
                if (question.Options != null)
                {
                    model.Options = question.Options.Select(o => new OptionViewModel
                    {
                        OptionId = o.OptionId,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList();
                    
                    // Find the index of the correct option for the radio buttons
                    var correctOption = model.Options.Select((o, i) => new { Option = o, Index = i }).FirstOrDefault(x => x.Option.IsCorrect);
                    if (correctOption != null)
                    {
                        model.CorrectOptionIndex = correctOption.Index;
                    }
                }
            }
            else
            {
                // Default options structure for a new question (e.g. 4 options)
                model.Options.Add(new OptionViewModel());
                model.Options.Add(new OptionViewModel());
                model.Options.Add(new OptionViewModel());
                model.Options.Add(new OptionViewModel());
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(QuestionUpsertViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Options == null || model.Options.Count < 2)
                {
                    ModelState.AddModelError("", "A question must have at least 2 options.");
                    return View(model);
                }

                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                var question = new Question
                {
                    QuestionId = model.QuestionId,
                    QuestionText = model.QuestionText,
                    IsActive = model.IsActive,
                    CreatedBy = currentUserId
                };

                // Save Question
                var newQuestionId = await _questionRepository.UpsertQuestionAsync(question);

                // Set correct option based on radio button selection
                for (int i = 0; i < model.Options.Count; i++)
                {
                    model.Options[i].IsCorrect = (i == model.CorrectOptionIndex);
                }

                // Save Options
                var options = model.Options.Select(o => new Option
                {
                    QuestionId = newQuestionId,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList();

                await _questionRepository.UpsertOptionsAsync(newQuestionId, options);

                TempData["SuccessMessage"] = "Question saved successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            await _questionRepository.ToggleStatusAsync(id);
            TempData["SuccessMessage"] = "Question status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
