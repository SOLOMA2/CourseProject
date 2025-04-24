using CourseProject.Models;
using CourseProject.Models.ViewsModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CourseProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration(RegistrationVM model)
        {
            if (ModelState.IsValid)
            {
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email уже зарегистрирован");
                    return View(model);
                }

                var user = new AppUser
                {
                    UserName = model.Name,
                    Email = model.Email,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var roleExist = await _roleManager.RoleExistsAsync("User");

                    if (!roleExist)
                    {
                        var role = new IdentityRole("User");
                        await _roleManager.CreateAsync(role);
                    }

                    await _userManager.AddToRoleAsync(user, "User");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Login", "Account");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || await _userManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError("", "Invalid credentials");
                    return View(model);
                }
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            user.LastLogin = DateTime.Now;
                            await _userManager.UpdateAsync(user);
                            return RedirectToAction("AdminPanel", "UserManager");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError(nameof(model.Email),"Invalid user or password");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailVM model)
        {
            if(ModelState.IsValid)
            {
                var email = await _userManager.FindByEmailAsync(model.Email);
                if (email != null)
                {
                    TempData["UserEmail"] = model.Email;
                    return RedirectToAction("ChangePaswword");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var email = TempData["UserEmail"]?.ToString();

            var model = new ChangePasswordVM
            {
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || await _userManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError("", "Invalid credentials");
                    return View(model);
                }
                if (user != null)
                {
                    var result = await _userManager.RemovePasswordAsync(user);

                    if (result.Succeeded)
                    {
                        result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
    