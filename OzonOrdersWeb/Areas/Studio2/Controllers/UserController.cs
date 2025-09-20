using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OzonDomains.ViewModels;
using OzonOrdersWeb.ViewModels.UserViewModels;
using OzonRepositories.Context;
using OzonRepositories.Context.Identity;
using Servcies.FiltersServcies.DataFilterManagers;
using Servcies.FiltersServcies.FilterModels;

namespace OzonOrdersWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Studio2")]
    public class UserController : Controller
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly OzonOrderContext _context;
        private readonly UserFilterManager _filter;
        public UserController(UserManager<CustomIdentityUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              OzonOrderContext context,
                              UserFilterManager userFilterManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _filter = userFilterManager;
        }
        public IActionResult Index() => View(_roleManager.Roles.ToList());

        public IActionResult CreateRole() => View();
        [HttpPost]
        public async Task<IActionResult> CreateRole(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(name));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(name);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await _roleManager.DeleteAsync(role);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UserList(int page = 1)
        {

            var usersWithUserAccess = _userManager.Users.Include(u => u.UserAccess).ToList();
            var accesses = _context.UserAccess.ToList();
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var accessDictionary = accesses.ToDictionary(a => a.Id);

            foreach (var user in usersWithUserAccess)
            {
                if (user.UserAccessId.HasValue && accessDictionary.ContainsKey(user.UserAccessId.Value))
                {
                    user.UserAccess = accessDictionary[user.UserAccessId.Value];
                }
            }

            ViewBag.Accesses = accesses;

            var filterDataString = HttpContext.Request.Cookies["UserFilterData"];
            var filterData = new UserFilterModel();
            if (!string.IsNullOrEmpty(filterDataString))
            {
                filterData = JsonConvert.DeserializeObject<UserFilterModel>(filterDataString);
                usersWithUserAccess = await _filter.FilterByFilterData(usersWithUserAccess, filterData);
            }
            ViewBag.Roles = roles; // Передаем список ролей в ViewBag

            return View(new PageViewModel<CustomIdentityUser, UserFilterModel>(usersWithUserAccess, page, 15, filterData));
        }

        [HttpPost]
        public async Task<IActionResult> UserList(UserFilterModel filterModel, int page = 1)
        {
            var serializedFilterData = JsonConvert.SerializeObject(filterModel);
            HttpContext.Response.Cookies.Append("UserFilterData", serializedFilterData);
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var usersWithUserAccess = _userManager.Users.Include(u => u.UserAccess).ToList();
            var accesses = _context.UserAccess.ToList();

            var accessDictionary = accesses.ToDictionary(a => a.Id);

            foreach (var user in usersWithUserAccess)
            {
                if (user.UserAccessId.HasValue && accessDictionary.ContainsKey(user.UserAccessId.Value))
                {
                    user.UserAccess = accessDictionary[user.UserAccessId.Value];
                }
            }

            ViewBag.Accesses = accesses;
            ViewBag.Roles = roles; // Передаем список ролей в ViewBag
            // Фильтруем пользователей по модели фильтрации
            var filteredUsers = await _filter.FilterByFilterData(usersWithUserAccess, filterModel);

            // Возвращаем обновленное представление
            return View(new PageViewModel<CustomIdentityUser, UserFilterModel>(filteredUsers, page, 15, filterModel));
        }



        public async Task<IActionResult> Edit(string userId)
        {
            // получаем пользователя
            CustomIdentityUser user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // получем список ролей пользователя
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                ChangeRoleViewModel model = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CustomIdentityUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByIdAsync(id);
                if (existingUser != null)
                {
                    existingUser.UserName = user.UserName;
                    existingUser.UserAccessId = user.UserAccessId;

                    var result = await _userManager.UpdateAsync(existingUser);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(UserList));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();
            var model = new ChangeRoleViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                UserRoles = userRoles,
                AllRoles = allRoles
            };
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserAccess(string id, CustomIdentityUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByIdAsync(id);
                if (existingUser != null)
                {
                    existingUser.UserAccessId = user.UserAccessId;

                    var result = await _userManager.UpdateAsync(existingUser);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(UserList));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    return NotFound();
                }
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new CustomIdentityUser { UserName = model.Username, Email = model.Username };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var roleExists = await _roleManager.RoleExistsAsync("user");
                    if (!roleExists)
                    {
                        await _roleManager.CreateAsync(new IdentityRole("user"));
                    }
                    await _userManager.AddToRoleAsync(user, "user");

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> SetUserAccess(string userId)
        {
            var viewModel = new SetUserAccessViewModel
            {
                UserAccesses = _context.UserAccess.ToList(),
                UserId = userId
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SetUserAccess(SetUserAccessViewModel viewModel)
        {

            if (viewModel.UserId != null && viewModel.SelectedUserAccessId != null)
            {
                // Логика для присвоения UserAccess к пользователю
                var user = await _userManager.FindByIdAsync(viewModel.UserId);
                var access = await _context.UserAccess.FindAsync(viewModel.SelectedUserAccessId);
                if (user != null)
                {
                    user.UserAccess = access;
                    await _userManager.UpdateAsync(user);
                    return RedirectToAction("UserList");
                }
            }

            viewModel.UserAccesses = await _context.UserAccess.ToListAsync();

            return View(viewModel);
        }

    }
}
