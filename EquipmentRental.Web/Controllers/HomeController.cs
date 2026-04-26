using EquipmentRental.Business;
using EquipmentRental.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentRental.Web.Controllers;

public class HomeController(IAppService appService) : Controller
{
    [Authorize]
    public IActionResult Index()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction(nameof(Admin));
        }
        return RedirectToAction("Index", "Equipment");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        var vm = await appService.GetDashboardAsync();
        return View(vm);
    }

    public IActionResult Error() => View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
}
