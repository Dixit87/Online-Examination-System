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
    public class ExamController : Controller
    {
        private readonly IExamRepository _examRepository;
        private readonly IExamTypeRepository _examTypeRepository;
        private readonly IQuestionRepository _questionRepository;

        public ExamController(
            IExamRepository examRepository,
            IExamTypeRepository examTypeRepository,
            IQuestionRepository questionRepository)
        {
            _examRepository = examRepository;
            _examTypeRepository = examTypeRepository;
            _questionRepository = questionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var exams = await _examRepository.GetAllAsync();
            return View(exams);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var model = new ExamUpsertViewModel();

            // Populate ExamTypes
            var examTypes = await _examTypeRepository.GetAllAsync();
            model.ExamTypeList = examTypes.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.Title,
                Value = e.ExamTypeId.ToString()
            });

            // Populate Questions
            var questions = await _questionRepository.GetAllAsync();
            var activeQuestions = questions.Where(q => q.IsActive).ToList();

            if (id.HasValue && id > 0)
            {
                var exam = await _examRepository.GetByIdAsync(id.Value);
                if (exam == null) return NotFound();

                model.ExamId = exam.ExamId;
                model.ExamTypeId = exam.ExamTypeId;
                model.ExamTitle = exam.ExamTitle;
                model.NoOfQuestions = exam.NoOfQuestions;
                model.PerQuestionMark = exam.PerQuestionMark;
                model.TotalMark = exam.TotalMark;
                model.PassingMark = exam.PassingMark;
                model.NegativeMark = exam.NegativeMark;
                model.StartDateTime = exam.StartDateTime;
                model.EndDateTime = exam.EndDateTime;
                model.IsActive = exam.IsActive;

                // Pre-populate the hidden field with existing question IDs
                if (exam.SelectedQuestionIds != null && exam.SelectedQuestionIds.Any())
                {
                    model.SelectedQuestionIds = string.Join(",", exam.SelectedQuestionIds);
                }
            }

            model.AvailableQuestions = activeQuestions.Select(q => new QuestionSelectionViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(ExamUpsertViewModel model)
        {
            // Repopulate Dropdown
            var examTypes = await _examTypeRepository.GetAllAsync();
            model.ExamTypeList = examTypes.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.Title,
                Value = e.ExamTypeId.ToString()
            });

            if (ModelState.IsValid)
            {
                var selectedQuestions = new List<int>();
                if (!string.IsNullOrEmpty(model.SelectedQuestionIds))
                {
                    selectedQuestions = model.SelectedQuestionIds.Split(',')
                                             .Where(x => !string.IsNullOrWhiteSpace(x))
                                             .Select(int.Parse)
                                             .ToList();
                }

                // Validation 1: Passing Mark <= Total Mark
                if (model.PassingMark > model.TotalMark)
                {
                    ModelState.AddModelError("PassingMark", "Passing Mark cannot be greater than Total Mark.");
                    
                    // Repopulate AvailableQuestions for view render if validation fails
                    var questions = await _questionRepository.GetAllAsync();
                    model.AvailableQuestions = questions.Where(q => q.IsActive).Select(q => new QuestionSelectionViewModel
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.QuestionText
                    }).ToList();
                    
                    return View(model);
                }

                // Validation 2: Selected Questions count must exactly match NoOfQuestions
                if (selectedQuestions.Count != model.NoOfQuestions)
                {
                    ModelState.AddModelError("", $"You must select exactly {model.NoOfQuestions} questions. You currently selected {selectedQuestions.Count}.");
                    
                    // Repopulate AvailableQuestions for view render if validation fails
                    var questions = await _questionRepository.GetAllAsync();
                    model.AvailableQuestions = questions.Where(q => q.IsActive).Select(q => new QuestionSelectionViewModel
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.QuestionText
                    }).ToList();
                    
                    return View(model);
                }

                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var exam = new Exam
                {
                    ExamId = model.ExamId,
                    ExamTypeId = model.ExamTypeId,
                    ExamTitle = model.ExamTitle,
                    NoOfQuestions = model.NoOfQuestions,
                    PerQuestionMark = model.PerQuestionMark,
                    TotalMark = model.TotalMark,
                    PassingMark = model.PassingMark,
                    NegativeMark = model.NegativeMark,
                    StartDateTime = model.StartDateTime,
                    EndDateTime = model.EndDateTime,
                    IsActive = model.IsActive,
                    CreatedBy = currentUserId
                };

                // Save Exam
                var newExamId = await _examRepository.UpsertExamAsync(exam);

                // Save Question Mappings
                await _examRepository.UpsertExamQuestionsAsync(newExamId, selectedQuestions);

                TempData["SuccessMessage"] = "Exam saved successfully.";
                return RedirectToAction(nameof(Index));
            }

            // Repopulate questions if model state is invalid initially
            var activeQs = await _questionRepository.GetAllAsync();
            model.AvailableQuestions = activeQs.Where(q => q.IsActive).Select(q => new QuestionSelectionViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            await _examRepository.ToggleStatusAsync(id);
            TempData["SuccessMessage"] = "Exam status updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Results(int id)
        {
            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null) return NotFound();

            var results = await _examRepository.GetResultsAsync(id);

            var model = new AdminExamResultViewModel
            {
                ExamId = id,
                ExamTitle = exam.ExamTitle,
                Results = results.ToList()
            };

            return View(model);
        }
    }
}
