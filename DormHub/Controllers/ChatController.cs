using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DormHub.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Czat";
            return View();
        }
    }
}
