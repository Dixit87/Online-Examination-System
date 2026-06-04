using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Online_Examination_System.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentPanelController : Controller
    {
        private readonly Online_Examination_System.Repositories.IStudentRepository _studentRepository;
        private readonly Online_Examination_System.Repositories.IExamRepository _examRepository;
        private readonly Online_Examination_System.Repositories.IUserRepository _userRepository;

        public StudentPanelController(
            Online_Examination_System.Repositories.IStudentRepository studentRepository,
            Online_Examination_System.Repositories.IExamRepository examRepository,
            Online_Examination_System.Repositories.IUserRepository userRepository)
        {
            _studentRepository = studentRepository;
            _examRepository = examRepository;
            _userRepository = userRepository;
        }

        public async System.Threading.Tasks.Task<IActionResult> Dashboard()
        {
            int studentId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var stats = await _studentRepository.GetDashboardStatsAsync(studentId);
            return View(stats);
        }

        public async System.Threading.Tasks.Task<IActionResult> Exams()
        {
            int studentId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var exams = await _studentRepository.GetStudentExamListAsync(studentId);
            return View(exams);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> TakeExam(int id)
        {
            int studentId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Get exam details
            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null) return NotFound();

            // Initialize or Resume
            int studentExamId = await _studentRepository.InitializeExamAsync(id, studentId);

            var vm = new Online_Examination_System.ViewModels.TakeExamViewModel
            {
                ExamId = id,
                StudentExamId = studentExamId,
                ExamTitle = exam.ExamTitle,
                TotalQuestions = exam.NoOfQuestions
            };

            return View(vm);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetExamData(int studentExamId, int examId)
        {
            var data = await _studentRepository.GetExamPayloadAsync(studentExamId, examId);
            return Json(data);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> SaveAnswer([FromBody] Online_Examination_System.ViewModels.SaveAnswerRequest request)
        {
            await _studentRepository.SaveAnswerAsync(request.StudentExamId, request.QuestionId, request.SelectedOptionId);
            return Ok();
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> SubmitExam(int studentExamId)
        {
            await _studentRepository.SubmitExamAsync(studentExamId);
            return RedirectToAction("Result", new { id = studentExamId });
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> Result(int id)
        {
            var result = await _studentRepository.GetExamResultAsync(id);
            if (result == null) return NotFound();

            return View(result);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> Profile()
        {
            int studentId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userRepository.GetByIdAsync(studentId);

            if (user == null) return NotFound();

            var vm = new Online_Examination_System.ViewModels.StudentProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Contact = user.Contact,
                Address = user.Address
            };

            return View(vm);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> Profile(Online_Examination_System.ViewModels.StudentProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int studentId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            string passwordHash = null;
            if (!string.IsNullOrEmpty(model.Password))
            {
                passwordHash = Online_Examination_System.Helpers.PasswordHelper.HashPassword(model.Password);
            }

            await _userRepository.UpdateStudentProfileAsync(studentId, model, passwordHash);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}
