using System.Security.Claims;
using EquipmentRental.Business;
using EquipmentRental.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentRental.Web.Controllers;

[Authorize]
public class RentalRequestsController(IAppService appService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Equipment = await appService.GetEquipmentAsync(null);
        return View(new RentalCreateInput());
    }

    [HttpPost]
    public async Task<IActionResult> Create(RentalCreateInput input)
    {
        ViewBag.Equipment = await appService.GetEquipmentAsync(null);
        if (!ModelState.IsValid) return View(input);
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await appService.CreateRequestAsync(userId, input);
        if (!result.ok)
        {
            ModelState.AddModelError(string.Empty, result.error ?? "Грешка при създаване на заявка.");
            return View(input);
        }
        return RedirectToAction(nameof(My));
    }

    public async Task<IActionResult> My()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return View(await appService.GetMyRequestsAsync(userId));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await appService.DeletePendingRequestAsync(userId, id);
        return RedirectToAction(nameof(My));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> All() => View(await appService.GetAllRequestsAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Status(int id, RentalRequestStatus status)
    {
        await appService.UpdateRequestStatusAsync(id, status);
        return RedirectToAction(nameof(All));
    }
}
