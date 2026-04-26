using EquipmentRental.Business;
using EquipmentRental.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentRental.Web.Controllers;

[Authorize]
public class EquipmentController(IAppService appService) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Search = search;
        return View(await appService.GetEquipmentAsync(search));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View(new EquipmentItem { Name = "", Description = "", ImageUrl = "" });

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(EquipmentItem item)
    {
        if (!ModelState.IsValid) return View(item);
        await appService.SaveEquipmentAsync(item);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await appService.GetEquipmentByIdAsync(id);
        if (item is null) return NotFound();
        return View(item);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, EquipmentItem item)
    {
        if (!ModelState.IsValid) return View(item);
        item.Id = id;
        await appService.SaveEquipmentAsync(item);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await appService.DeleteEquipmentAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
