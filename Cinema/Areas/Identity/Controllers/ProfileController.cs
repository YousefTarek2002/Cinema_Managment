using Cinema.Models;
using Cinema.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cinema.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var vm = new ApplicationUserVM
            {
                FullName = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ApplicationUserVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();

            user.Name = vm.FullName;
            user.Email = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            user.Address = vm.Address;

            await _userManager.UpdateAsync(user);

            TempData["success-notification"] = "✅ تم تحديث البيانات بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(ApplicationUserVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(vm.CurrentPassword) || string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                TempData["error-notification"] = "⚠️ يجب إدخال كلمة السر الحالية والجديدة";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);

            if (!result.Succeeded)
            {
                TempData["error-notification"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            TempData["success-notification"] = "✅ تم تغيير كلمة السر بنجاح";
            return RedirectToAction(nameof(Index));
        }
    }
}
