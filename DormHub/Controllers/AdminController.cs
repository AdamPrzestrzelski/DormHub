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
        => Redirect("/rodzaj_pokoju");

    [HttpGet("Pokoje")]
    public IActionResult Pokoje()
        => Redirect("/pokoje");

    [HttpGet("Budynki")]
    public IActionResult Budynki()
        => Redirect("/budynki");

    [HttpGet("Usterki")]
    public IActionResult Usterki()
        => Redirect("/usterki");

    [HttpGet("Osoby")]
    public IActionResult Osoby()
        => Redirect("/osoby");

    [HttpGet("Mieszkancy")]
    public IActionResult Mieszkancy()
        => Redirect("/mieszkancy");

    [HttpGet("Wnioski")]
    public IActionResult Wnioski()
        => Redirect("/wnioski");

    [HttpGet("Platnosci")]
    public IActionResult Platnosci()
        => Redirect("/platnosci");

    [HttpGet("Ogloszenia")]
    public IActionResult Ogloszenia()
        => Redirect("/ogloszenia");
}