using Microsoft.AspNetCore.Mvc;

namespace Petfolio.API.Controllers;

public class Pet : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}