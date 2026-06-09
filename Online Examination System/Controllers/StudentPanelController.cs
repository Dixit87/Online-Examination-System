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
        private readonly Online_Examination_System.Repositories.ISessionRepository _sessionRepository;
        private readonly Online_Examination_System.Repositories.IChapterRepository _chapterRepository;
        private readonly Online_Examination_System.Repositories.ITopicRepository _topicRepository;

        public StudentPanelController(
            Online_Examination_System.Repositories.IStudentRepository studentRepository,
            Online_Examination_System.Repositories.IExamRepository examRepository,
            Online_Examination_System.Repositories.IUserRepository userRepository,
            Online_Examination_System.Repositories.ISessionRepository sessionRepository,
            Online_Examination_System.Repositories.IChapterRepository chapterRepository,
            Online_Examination_System.Repositories.ITopicRepository topicRepository)
        {
            _studentRepository = studentRepository;
            _examRepository = examRepository;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _chapterRepository = chapterRepository;
            _topicRepository = topicRepository;
        }

        public async System.Threading.Tasks.Task<IActionResult> Dashboard()
        {
            int studentId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var stats = await _studentRepository.GetDashboardStatsAsync(studentId);
            return View(stats);
        }

        public async System.Threading.Tasks.Task<IActionResult> Sessions()
        {
            var sessions = await _sessionRepository.GetAllAsync();
            return View(System.Linq.Enumerable.Where(sessions, s => s.Status));
        }

        public async System.Threading.Tasks.Task<IActionResult> Chapters(int sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null || !session.Status) return NotFound();
            
            ViewBag.SessionTitle = session.Title;
            
            var chapters = await _chapterRepository.GetAllAsync();
            return View(System.Linq.Enumerable.Where(chapters, c => c.SessionId == sessionId && c.Status));
        }

        public async System.Threading.Tasks.Task<IActionResult> Topics(int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null || !chapter.Status) return NotFound();
            
            var session = await _sessionRepository.GetByIdAsync(chapter.SessionId);
            ViewBag.SessionTitle = session.Title;
            ViewBag.ChapterTitle = chapter.Title;
            
            var topics = await _topicRepository.GetAllAsync();
            return View(System.Linq.Enumerable.Where(topics, t => t.ChapterId == chapterId && t.Status));
        }

        public async System.Threading.Tasks.Task<IActionResult> TopicDetails(int topicId)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null || !topic.Status) return NotFound();
            
            var chapter = await _chapterRepository.GetByIdAsync(topic.ChapterId);
            var session = await _sessionRepository.GetByIdAsync(topic.SessionId);
            ViewBag.SessionTitle = session.Title;
            ViewBag.ChapterTitle = chapter.Title;
            
            // Only fetch details if it's Accordion or Multi Slider
            if (topic.TopicType != "General")
            {
                var details = await _topicRepository.GetTopicDetailsByTopicIdAsync(topicId);
                topic.TopicDetails = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(details, d => d.Status));
            }
            
            return View(topic);
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

            var now = System.DateTime.Now;
            if (exam.StartDateTime.HasValue && now < exam.StartDateTime.Value)
            {
                TempData["ErrorMessage"] = $"This exam has not started yet. It will start at {exam.StartDateTime.Value:dd MMM yyyy, hh:mm tt}.";
                return RedirectToAction("Exams");
            }

            if (exam.EndDateTime.HasValue && now > exam.EndDateTime.Value)
            {
                TempData["ErrorMessage"] = "This exam has ended and is no longer available.";
                return RedirectToAction("Exams");
            }

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
