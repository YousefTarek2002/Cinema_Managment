using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Cinema.Utilites;
using Cinema.ViewModels;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Identity")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleController(RoleManager<IdentityRole> roleManager) => _roleManager = roleManager;

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateRoleVM());

        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleVM model)
        {
            if (!ModelState.IsValid) return View(model);
            if (await _roleManager.RoleExistsAsync(model.Name))
            {
                ModelState.AddModelError("", "Role already exists");
                return View(model);
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(model.Name));
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(model);
            }

            TempData["success-notification"] = "Role created";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                TempData["error-notification"] = "Can't delete role";
            }
            else
            {
                TempData["success-notification"] = "Role deleted";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
