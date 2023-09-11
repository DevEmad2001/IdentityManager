using IdentityManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityManager.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager; // dependncy injection
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole>_roleManager ;

        public AccountController(UserManager<IdentityUser> userManager , SignInManager<IdentityUser>signInManager, IEmailSender emailSender,
            RoleManager<IdentityRole>roleManager) //Constractor
        {
           _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]//This means that everyone can access this method (Register)
        public async Task<IActionResult> Register(string returnurl=null) // to Get data from client for used UI 
        {
            if (!await _roleManager.RoleExistsAsync("Manager")) // this condition  works on check of exist the adman inside database or not 
            {
                //create roles
                await _roleManager.CreateAsync(new IdentityRole("Manager"));//create admin role
                await _roleManager.CreateAsync(new IdentityRole("Employee"));//create User role
            }

            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem()
             {
                Value="Manager",
                Text="Manager"

            });
            listItems.Add(new SelectListItem()
            {
                Value = "Employee",
                Text = "Employee"

            });
            ViewData["ReturnUrl"] = returnurl;
            RegisterViewModel registerViewModel = new RegisterViewModel()
            {
                RoleList = listItems
            };

            return View(registerViewModel); //To show view page 
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model,string returnurl=null)  //these are server side validation
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            if (ModelState.IsValid) 
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Name = model.Name };
                var result=await _userManager.CreateAsync(user,model.Password);
                if (result.Succeeded) 
                {
                    if (model.RoleSelected != null && model.RoleSelected.Length > 0 && model.RoleSelected == "Manager")
                    {
                        await _userManager.AddToRoleAsync(user, "Manager");
                    }
                    else 
                    {
                        await _userManager.AddToRoleAsync(user, "Employee");
                    }
                    var code=await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var callbackurl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    await _emailSender.SendEmailAsync(model.Email, "Cobfirm your account - Identity Manager",
                        "Please confirm your account by clicking here : <a herf=\"" + callbackurl + "\">link</a>");

                    await _signInManager.SignInAsync(user, isPersistent: false); // You need to login every time you use the website page 
                    return LocalRedirect(returnurl);
                }
                AddErrors(result);
            }
            // this code It was written to solve a problem in server side for Repeat Email 
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem()
            {
                Value = "Manager",
                Text = "Manager"

            });
            listItems.Add(new SelectListItem()
            {
                Value = "Employee",
                Text = "Employee"

            });
            model.RoleList=listItems;
            return View(model);
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnurl=null) // to Get data from client for used UI 
        {
            ViewData["ReturnUrl"] = returnurl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] // It is used to increase protection(security)
        public async Task<IActionResult> Login(LoginViewModel model,string returnurl=null)  //these are server side validation
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");// if returnurl is null It will set the value of the current page
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return  LocalRedirect(returnurl); // To prevent any Error 
                }
                if (result.IsLockedOut) 
                {
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
           
            }

            return View(model);
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] // security reason
        public async Task<IActionResult> LogOff()  //these are server side validation
        {
           await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");  /*nameof(Action method (inside controller)),controller name */

        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId,string code) 
        {
            if (userId == null || code == null) 
            {
                return View("Error");
            }
        var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                return View("Error");
            }
            var result= await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfiramEmail":"Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() // to Get data from client for used UI 
        {
         
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] // It is used to increase protection(security)
        public async Task<IActionResult> ForgotPassword(ForgetPasswordViewModel model)  //these are server side validation
        {
            if (ModelState.IsValid) 
            {
                var user= await _userManager.FindByEmailAsync(model.Email);
                if (user == null) 
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackurl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email,"Reset Password - Identity Manager",
                    "Please reset your password by clicking here : <a herf=\""+callbackurl+"\">link</a>");

                return RedirectToAction("ForgotPasswordConfirmation");
                }
     


            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation() 
        {
            return View();
        }
        private void AddErrors(IdentityResult result) /// if account is fake..
        {
            foreach (var error in result.Errors) 
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code=null) // to Get data from client for used UI 
        {
            return code==null? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] // It is used to increase protection(security)
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)  //these are server side validation
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation"); // To prevent any Error 
                }

                AddErrors(result);
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
