using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Examination_System.Repositories;

namespace Online_Examination_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepository;

        public StudentController(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _studentRepository.GetAllStudentsAsync();
            return View(students);
        }

        public async Task<IActionResult> Details(int id)
        {
            var studentDetails = await _studentRepository.GetStudentDetailsAsync(id);
            if (studentDetails == null)
            {
                return NotFound();
            }
            return View(studentDetails);
        }
    }
}
