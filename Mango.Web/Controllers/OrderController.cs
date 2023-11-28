using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers;

public class OrderController : Controller
{
    [Authorize]
    public IActionResult OrderIndex()
    {
        return View();
    }
}
