using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestoranProjesi.Models.Entities;
using RestoranProjesi.Models.ViewModels;

namespace RestoranProjesi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    else
         
                        return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Çok fazla başarısız deneme! Hesabınız 15 dakika boyunca kilitlendi.");
                    return View(model);
                }
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingPhoneUser = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == model.PhoneNumber);
                if (existingPhoneUser != null)
                {
                    ModelState.AddModelError("PhoneNumber", "Bu telefon numarası ile kayıtlı başka bir hesap bulunmaktadır.");
                    return View(model);
                }

                var user = new AppUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, PhoneNumber = model.PhoneNumber };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Varsayılan olarak "User" rolü ver
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                // Handle Password Change
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if (string.IsNullOrEmpty(model.CurrentPassword))
                    {
                        ModelState.AddModelError(string.Empty, "Şifrenizi değiştirmek için mevcut şifrenizi girmelisiniz.");
                        return View(model);
                    }

                    var passResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    if (!passResult.Succeeded)
                    {
                        foreach (var error in passResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }

                // Handle Email Change
                if (user.Email != model.Email)
                {
                    // Generate mock 6-digit code
                    var code = new Random().Next(100000, 999999).ToString();
                    TempData["VerificationCode"] = code; // In a real app, this is sent via email and stored in db/cache
                    TempData["NewEmail"] = model.Email;
                    
                    // Show success alert in verify page
                    TempData["SuccessMessage"] = $"Onay kodu e-postanıza gönderildi! (Test Kodu: {code})";
                    return RedirectToAction("VerifyEmail");
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                    await _signInManager.RefreshSignInAsync(user);
                    return RedirectToAction("Profile");
                }

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            if (TempData["NewEmail"] == null) return RedirectToAction("Profile");
            ViewBag.NewEmail = TempData["NewEmail"];
            TempData.Keep("NewEmail");
            TempData.Keep("VerificationCode");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(string code)
        {
            string expectedCode = TempData["VerificationCode"] as string;
            string newEmail = TempData["NewEmail"] as string;

            if (code == expectedCode && !string.IsNullOrEmpty(newEmail))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    await _userManager.SetEmailAsync(user, newEmail);
                    await _userManager.SetUserNameAsync(user, newEmail);
                    await _userManager.UpdateAsync(user);
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "E-posta adresiniz başarıyla güncellendi!";
                    return RedirectToAction("Profile");
                }
            }
            
            ModelState.AddModelError(string.Empty, "Geçersiz veya hatalı onay kodu.");
            ViewBag.NewEmail = newEmail;
            TempData.Keep("NewEmail");
            TempData.Keep("VerificationCode");
            return View();
        }

        [HttpGet]
        public IActionResult AddLinkedAccount()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddLinkedAccount(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var targetUser = await _userManager.FindByEmailAsync(model.Email);
                if (targetUser != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(targetUser, model.Password, false);
                    if (result.Succeeded)
                    {
                        var linkedStr = Request.Cookies["LinkedAccounts"] ?? "";
                        var linkedList = linkedStr.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();
                        
                        if (!linkedList.Contains(targetUser.Email!))
                            linkedList.Add(targetUser.Email!);

                        var currentUser = await _userManager.GetUserAsync(User);
                        if (currentUser != null && !linkedList.Contains(currentUser.Email!))
                            linkedList.Add(currentUser.Email!);

                        Response.Cookies.Append("LinkedAccounts", string.Join(",", linkedList), new CookieOptions { Expires = DateTime.Now.AddDays(30) });

                        await _signInManager.SignOutAsync();
                        await _signInManager.SignInAsync(targetUser, isPersistent: true);
                        
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SwitchAccount(string email)
        {
            var linkedStr = Request.Cookies["LinkedAccounts"] ?? "";
            var linkedList = linkedStr.Split(',').ToList();
            if (linkedList.Contains(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(user, isPersistent: true);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult RemoveLinkedAccount(string email)
        {
            var linkedStr = Request.Cookies["LinkedAccounts"] ?? "";
            var linkedList = linkedStr.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (linkedList.Contains(email))
            {
                linkedList.Remove(email);
                if (linkedList.Any())
                {
                    Response.Cookies.Append("LinkedAccounts", string.Join(",", linkedList), new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                }
                else
                {
                    Response.Cookies.Delete("LinkedAccounts");
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _signInManager.SignOutAsync();
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var currentUser = User.Identity?.Name;
            await _signInManager.SignOutAsync();

            var linkedStr = Request.Cookies["LinkedAccounts"] ?? "";
            var linkedList = linkedStr.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();
            
            if (currentUser != null && linkedList.Contains(currentUser))
            {
                linkedList.Remove(currentUser);
            }

            if (linkedList.Any())
            {
                Response.Cookies.Append("LinkedAccounts", string.Join(",", linkedList), new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                
                var nextUserEmail = linkedList.First();
                var nextUser = await _userManager.FindByEmailAsync(nextUserEmail);
                if (nextUser != null)
                {
                    await _signInManager.SignInAsync(nextUser, isPersistent: true);
                }
            }
            else
            {
                Response.Cookies.Delete("LinkedAccounts");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
