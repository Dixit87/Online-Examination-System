using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
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
            TempData["SuccessMessage"] = "Question status updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Analytics()
        {
            var analytics = await _questionRepository.GetQuestionAnalyticsAsync();
            return View(analytics);
        }

        [HttpGet]
        public IActionResult BulkUpload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid Excel file.");
                return View();
            }

            if (!file.FileName.EndsWith(".xlsx"))
            {
                ModelState.AddModelError("", "Only .xlsx files are supported.");
                return View();
            }

            try
            {
                OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("OES Admin");
                using var stream = new System.IO.MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new OfficeOpenXml.ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    ModelState.AddModelError("", "The Excel file is empty.");
                    return View();
                }

                int rowCount = worksheet.Dimension.Rows;
                int importedCount = 0;

                // Loop through rows (skip header row 1)
                for (int row = 2; row <= rowCount; row++)
                {
                    string questionText = worksheet.Cells[row, 1].Text?.Trim();
                    string opt1 = worksheet.Cells[row, 2].Text?.Trim();
                    string opt2 = worksheet.Cells[row, 3].Text?.Trim();
                    string opt3 = worksheet.Cells[row, 4].Text?.Trim();
                    string opt4 = worksheet.Cells[row, 5].Text?.Trim();
                    string correctOptStr = worksheet.Cells[row, 6].Text?.Trim();

                    if (string.IsNullOrEmpty(questionText) || string.IsNullOrEmpty(correctOptStr))
                        continue;

                    if (!int.TryParse(correctOptStr, out int correctOptNum) || correctOptNum < 1 || correctOptNum > 4)
                        continue;

                    // Insert Question
                    var newQuestion = new Question
                    {
                        QuestionText = questionText,
                        IsActive = true,
                        CreatedBy = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0")
                    };

                    int questionId = await _questionRepository.UpsertQuestionAsync(newQuestion);

                    // Insert Options
                    var options = new List<Option>();
                    if (!string.IsNullOrEmpty(opt1)) options.Add(new Option { OptionText = opt1, IsCorrect = (correctOptNum == 1) });
                    if (!string.IsNullOrEmpty(opt2)) options.Add(new Option { OptionText = opt2, IsCorrect = (correctOptNum == 2) });
                    if (!string.IsNullOrEmpty(opt3)) options.Add(new Option { OptionText = opt3, IsCorrect = (correctOptNum == 3) });
                    if (!string.IsNullOrEmpty(opt4)) options.Add(new Option { OptionText = opt4, IsCorrect = (correctOptNum == 4) });

                    if (options.Count > 0)
                    {
                        await _questionRepository.UpsertOptionsAsync(questionId, options);
                        importedCount++;
                    }
                }

                TempData["SuccessMessage"] = $"{importedCount} questions successfully imported from Excel!";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while importing: " + ex.Message);
                return View();
            }
        }
    }
}
