using IdentityManager.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityManager.Controllers
{
    public class RolesController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var roles = _db.Roles.ToList(); // to show all roles inside role table in database 
            return View(roles);
        }

        [HttpGet]
        public IActionResult Upsert(string id)
        {
            if (string.IsNullOrEmpty(id)) //to check the user updating or not 
            {
                return View();
            }
            else
            {
                // update
                var objFromDb = _db.Roles.FirstOrDefault(u => u.Id == id); //to check the retive data from database is true
                return View(objFromDb);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(IdentityRole roleObj)
        {
            if (await _roleManager.RoleExistsAsync(roleObj.Name))//this condition works check table inside database if that name is exist we have an error
            {
                //error
                TempData[SD.Error] = "Role Already Exists";
                return RedirectToAction(nameof(Index));
            }
            if (string.IsNullOrEmpty(roleObj.Id))//this another condition for check if role null or empty 
            {
                //create
                await _roleManager.CreateAsync(new IdentityRole() { Name = roleObj.Name });
                TempData[SD.Success] = "Role Created Successfully";
            }
            else
            {
                //update
                var objRoleFromDb = _db.Roles.FirstOrDefault(u => u.Id == roleObj.Id);
                if (objRoleFromDb == null) 
                {
                    TempData[SD.Error] = "Role Not Found";
                    return RedirectToAction(nameof(Index));
                }
                objRoleFromDb.Name = roleObj.Name;
                objRoleFromDb.NormalizedName = roleObj.Name.ToUpper();
                var result = await _roleManager.UpdateAsync(objRoleFromDb); // this line works update all roles(roleTable) inside database 
                TempData[SD.Success] = "Role Updated Successfully";
            }
            return RedirectToAction(nameof(Index));
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var objFromDb = _db.Roles.FirstOrDefault(u => u.Id == id);
            var userRolesForThisRole = _db.UserRoles.Where(u => u.RoleId == id).Count(); // return number of user that have particuler role assigend
            if (objFromDb == null)
            {
                TempData[SD.Error] = "Role not Found.";
                return RedirectToAction(nameof(Index));
            }
            if (userRolesForThisRole > 0) 
            {
                //error
                TempData[SD.Error] = "Cannot Delete this Role , Since there are users assigend to this role";
                return RedirectToAction(nameof(Index));
            }
            await _roleManager.DeleteAsync(objFromDb);
            TempData[SD.Success] = "Role Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
