using Cinema.Models;
using Cinema.Repositories.IRepositories;
using Cinema.Utilites;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Cinema.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IRepository<ApplicationUserOTP> applicationUserOTPRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOTPRepository = applicationUserOTPRepository;
        }

        // ======================== Register ========================
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            var user = new ApplicationUser
            {
                Name = registerVM.Name,
                Email = registerVM.Email,
                UserName = registerVM.UserName
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(registerVM);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = Url.Action(nameof(ConfirmEmail), "Account",
                new { area = "Identity", id = user.Id, token = encodedToken }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email,
                "Cinema - Confirm Your Email",
                $"<h1>Please confirm your email by clicking <a href='{link}'>here</a></h1>");

            // ✅ Add role to user
            await _userManager.AddToRoleAsync(user, SD.USER_ROLE);

            TempData["success-notification"] = "Confirmation email sent successfully!";
            return Redirect("/Identity/Account/Login");
        }
     
        // ======================== Login ========================
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);

            var user = await _userManager.FindByNameAsync(loginVM.UserNameOREmail)
                       ?? await _userManager.FindByEmailAsync(loginVM.UserNameOREmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email or Password");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    ModelState.AddModelError(string.Empty, "Too many attempts, try again later.");
                else if (result.IsNotAllowed)
                    ModelState.AddModelError(string.Empty, "Please confirm your email first!");
                else
                    ModelState.AddModelError(string.Empty, "Invalid User Name / Email or Password");

                return View(loginVM);
            }

            TempData["success-notification"] = "Login successfully!";
            return RedirectToAction("Index", "Home", new { area = "Dashboard" });
        }

        // ======================== Confirm Email ========================
        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["error-notification"] = "User not found.";
                return Redirect("/");
            }

            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch
            {
                TempData["error-notification"] = "Invalid token format.";
                return Redirect("/Identity/Account/Login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                TempData["error-notification"] = "Invalid or expired token.";
            else
                TempData["success-notification"] = "Email confirmed successfully! You can now login.";

            return Redirect("/Identity/Account/Login");
        }

        // ======================== Resend Email Confirmation ========================
        public IActionResult ResendEmailConfirmation() => View();

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendVM)
        {
            if (!ModelState.IsValid) return View(resendVM);

            var user = await _userManager.FindByNameAsync(resendVM.UserNameOREmail)
                       ?? await _userManager.FindByEmailAsync(resendVM.UserNameOREmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email");
                return View(resendVM);
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Email is already confirmed!");
                return View(resendVM);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var link = Url.Action(nameof(ConfirmEmail), "Account",
                new { area = "Identity", id = user.Id, token = encodedToken }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email,
                "Cinema - Confirm Your Email Again",
                $"<h1>Please confirm your email by clicking <a href='{link}'>here</a></h1>");

            TempData["success-notification"] = "Confirmation email resent successfully!";
            return Redirect("/Identity/Account/Login");
        }

        // ======================== Forget Password ========================
        public IActionResult ForgetPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return View(forgetPasswordVM);

            var user = await _userManager.FindByNameAsync(forgetPasswordVM.UserNameOREmail)
                       ?? await _userManager.FindByEmailAsync(forgetPasswordVM.UserNameOREmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email");
                return View(forgetPasswordVM);
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var userOTPs = await _applicationUserOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id);
            var totalCount = userOTPs.Count(e => (DateTime.UtcNow - e.CreateAt).TotalHours < 24);

            if (totalCount > 5)
            {
                ModelState.AddModelError(string.Empty, "Please try again later. Too many attempts.");
                return View(forgetPasswordVM);
            }

            var userOtp = new ApplicationUserOTP
            {
                ApplicationUserId = user.Id,
                CreateAt = DateTime.UtcNow,
                IsValid = true,
                Id = Guid.NewGuid().ToString(),
                OTP = otp,
                ValidTo = DateTime.UtcNow.AddMinutes(30)
            };

            await _applicationUserOTPRepository.CreateAsync(userOtp, cancellationToken);
            await _applicationUserOTPRepository.CommitAsync(cancellationToken);

            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailSender.SendEmailAsync(user.Email,
                    "Cinema - Forget Password",
                    $"<h1>Use this OTP: {otp} to validate your account. Don't share it.</h1>");
            }

            TempData["success-notification"] = "OTP has been sent to your email.";
            TempData["From-ForgetPassword"] = Guid.NewGuid().ToString();

            return RedirectToAction("ValidateOTP", new { userId = user.Id });
        }

        public IActionResult ValidateOTP(string userId)
        {
            if (TempData["From-ForgetPassword"] == null) return NotFound();
            return View(new ValidateOTP { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTP validateOTP)
        {
            if (!ModelState.IsValid) return View(validateOTP);

            var validOTP = await _applicationUserOTPRepository.GetOneAsync(
                e => e.ApplicationUserId == validateOTP.UserId && e.IsValid && e.ValidTo > DateTime.UtcNow);

            if (validOTP == null)
            {
                TempData["error-notification"] = "Invalid OTP";
                return RedirectToAction(nameof(ValidateOTP), new { userId = validateOTP.UserId });
            }

            TempData["From-ValidateOTP"] = Guid.NewGuid().ToString();
            return RedirectToAction("NewPassword", new { userId = validateOTP.UserId });
        }

        // ======================== New Password ========================
        public IActionResult NewPassword(string userId)
        {
            if (TempData["From-ValidateOTP"] == null) return NotFound();
            return View(new NewPasswordVM { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordVM newPasswordVM)
        {
            if (!ModelState.IsValid) return View(newPasswordVM);

            var user = await _userManager.FindByIdAsync(newPasswordVM.UserId);
            if (user == null)
            {
                TempData["error-notification"] = "User Not Found";
                return RedirectToAction(nameof(NewPassword), new { userId = newPasswordVM.UserId });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newPasswordVM.Password);

            TempData["success-notification"] = "Password changed successfully!";
            return Redirect("/Identity/Account/Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Dashboard/Home/Index");
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { area = "Identity", returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/Dashboard/Home/Index");

            if (remoteError != null)
            {
                TempData["error-notification"] = $"External provider error: {remoteError}";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var loginResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (loginResult.Succeeded)
                return LocalRedirect(returnUrl);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0];

            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    var r = new Random().Next(1000, 9999);
                    user = new ApplicationUser
                    {
                        UserName = username.Replace(" ", "") + r,
                        Email = email,
                        Name = username,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        TempData["error-notification"] = "Error creating account.";
                        return RedirectToAction("Login", "Account", new { area = "Identity" });
                    }
                }

                if (!(await _userManager.GetLoginsAsync(user))
                    .Any(l => l.LoginProvider == info.LoginProvider))
                {
                    await _userManager.AddLoginAsync(user, info);
                }

                await _signInManager.SignInAsync(user, false);
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }


        // ======================== Logout ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["success-notification"] = "Logged out successfully!";

            // رجعه لصفحة تسجيل الدخول
            return Redirect("/Dashboard/Home");
        }



    }
}
