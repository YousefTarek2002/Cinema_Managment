using Cinema.Models;
using Cinema.Utilites;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Identity")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Index with search & paging (simple)
        public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 25)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                usersQuery = usersQuery.Where(x =>
                    x.UserName.Contains(q) ||
                    x.Email.Contains(q) ||
                    x.Name.Contains(q));
            }

            int total = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new List<UserWithRolesVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserWithRolesVM
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            ViewData["Query"] = q;
            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["Total"] = total;

            return View(model);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View(new ApplicationUser());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApplicationUser user, string password)
        {
            if (!ModelState.IsValid) return View(user);

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(user);
            }

            await _userManager.AddToRoleAsync(user, SD.USER_ROLE);
            TempData["success-notification"] = "User Created Successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ApplicationUser model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.Name = model.Name;
            user.Email = model.Email;
            user.UserName = model.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(model);
            }

            TempData["success-notification"] = "User Updated Successfully";
            return RedirectToAction(nameof(Index));
        }

        // Lock / Unlock
        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, SD.SUPER_ADMIN_ROLE))
            {
                TempData["error-notification"] = "You can not lock Super Admin account";
                return RedirectToAction(nameof(Index));
            }

            bool isLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow;
            if (isLocked)
            {
                user.LockoutEnd = null;
                TempData["success-notification"] = "User unlocked";
            }
            else
            {
                user.LockoutEnd = DateTime.UtcNow.AddMonths(1);
                TempData["success-notification"] = "User locked";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        // Assign Roles (GET)
        [HttpGet]
        public async Task<IActionResult> AssignRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var vm = new UserRolesVM
            {
                UserId = user.Id,
                UserName = user.UserName
            };

            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            vm.Roles = allRoles.Select(r => new RoleCheckboxItem
            {
                RoleName = r,
                Selected = userRoles.Contains(r)
            }).ToList();

            return View(vm);
        }

        // Assign Roles (POST)
        [HttpPost]
        public async Task<IActionResult> AssignRoles(UserRolesVM model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var selectedRoles = model.Roles.Where(r => r.Selected).Select(r => r.RoleName).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var toAdd = selectedRoles.Except(userRoles).ToList();
            var toRemove = userRoles.Except(selectedRoles).ToList();

            if (toAdd.Any())
                await _userManager.AddToRolesAsync(user, toAdd);

            if (toRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, toRemove);

            TempData["success-notification"] = "User roles updated";
            return RedirectToAction(nameof(Index));
        }
    }
}
