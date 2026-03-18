using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Library.MVC.Controllers.Admin
{
        [Authorize(Roles = "Admin")]
        public class RolesController : Controller
        {
            private readonly RoleManager<IdentityRole> _roleManager;

            public RolesController(RoleManager<IdentityRole> roleManager)
            {
                _roleManager = roleManager;
            }

            public IActionResult Index()
            {
                var roles = _roleManager.Roles.ToList();
                return View(roles);
            }

            [HttpPost]
            public async Task<IActionResult> Create(string roleName)
            {
                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            [HttpPost]
            public async Task<IActionResult> Delete(string roleId)
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role != null)
                {
                    await _roleManager.DeleteAsync(role);
                }
                return RedirectToAction(nameof(Index));
            }
        }
    }
