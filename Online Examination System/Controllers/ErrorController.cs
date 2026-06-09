using Microsoft.AspNetCore.Mvc;

namespace Online_Examination_System.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("Error404");
                default:
                    return View("Error500");
            }
        }

        [Route("Error/500")]
        public IActionResult Error500()
        {
            return View("Error500");
        }
    }
}
