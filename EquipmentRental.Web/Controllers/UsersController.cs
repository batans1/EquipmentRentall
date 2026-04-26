using EquipmentRental.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentRental.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController(IAppService appService) : Controller
{
    public async Task<IActionResult> Index() => View(await appService.GetUsersAsync());

    [HttpGet]
    public IActionResult Create() => View(new RegisterInput());

    [HttpPost]
    public async Task<IActionResult> Create(RegisterInput input)
    {
        if (!ModelState.IsValid) return View(input);
        var ok = await appService.CreateUserAsync(input);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Потребителското име е заето.");
            return View(input);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await appService.GetUserAsync(id);
        if (user is null) return NotFound();
        return View(new RegisterInput
        {
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, RegisterInput input)
    {
        if (!ModelState.IsValid) return View(input);
        var ok = await appService.UpdateUserAsync(id, input);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Неуспешна промяна.");
            return View(input);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await appService.DeleteUserAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
