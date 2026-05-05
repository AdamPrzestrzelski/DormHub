using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
[Route("Admin")]
public class AdminController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View("Admin");
    }

    [HttpGet("RodzajPokoju")]
    public IActionResult RodzajPokoju()
        => Redirect("https://localhost:7289/rodzaj_pokoju");

    [HttpGet("Pokoje")]
    public IActionResult Pokoje()
        => Redirect("https://localhost:7289/pokoje");

    [HttpGet("Budynki")]
    public IActionResult Budynki()
        => Redirect("https://localhost:7289/budynki");

    [HttpGet("Usterki")]
    public IActionResult Usterki()
        => Redirect("https://localhost:7289/usterki");

    [HttpGet("Osoby")]
    public IActionResult Osoby()
        => Redirect("https://localhost:7289/osoby");

    [HttpGet("Mieszkancy")]
    public IActionResult Mieszkancy()
        => Redirect("https://localhost:7289/mieszkancy");

    [HttpGet("Wnioski")]
    public IActionResult Wnioski()
        => Redirect("https://localhost:7289/wnioski");
}