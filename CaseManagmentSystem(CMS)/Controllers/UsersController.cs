using CaseManagementSystem.Constants;
using CaseManagementSystem.Services;
using CaseManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagementSystem.Controllers
{
    [Authorize(Roles = RoleNames.Supervisor)]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;


        public UsersController(
            IUserService userService)
        {
            _userService = userService;
        }


        // =====================================================
        // USER LIST
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users =
                await _userService
                    .GetUserListAsync();

            return View(users);
        }


        // =====================================================
        // CREATE
        // =====================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(
                new CreateUserViewModel()
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var result =
                await _userService
                    .CreateUserAsync(model);


            if (result.Succeeded)
            {
                TempData["UserSuccess"] =
                    "User created successfully.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            AddCreateErrors(result);

            return View(model);
        }


        // =====================================================
        // EDIT
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            string id)
        {
            var model =
                await _userService
                    .GetUserForEditAsync(id);


            if (model == null)
            {
                return NotFound();
            }


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var result =
                await _userService
                    .UpdateUserAsync(model);


            if (result.Succeeded)
            {
                TempData["UserSuccess"] =
                    "User updated successfully.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            AddEditErrors(result);

            return View(model);
        }


        // =====================================================
        // CREATE ERROR MAPPING
        // =====================================================

        private void AddCreateErrors(
            IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                var field =
                    GetIdentityErrorField(
                        error
                    );


                /*
                 * Password errors belong directly under
                 * the Password field.
                 */

                if (IsPasswordError(error))
                {
                    field =
                        nameof(
                            CreateUserViewModel.Password
                        );
                }


                ModelState.AddModelError(
                    field,
                    error.Description
                );
            }
        }


        // =====================================================
        // EDIT ERROR MAPPING
        // =====================================================

        private void AddEditErrors(
            IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                var field =
                    GetIdentityErrorField(
                        error
                    );


                ModelState.AddModelError(
                    field,
                    error.Description
                );
            }
        }


        // =====================================================
        // IDENTITY ERROR -> FIELD
        // =====================================================

        private static string GetIdentityErrorField(
            IdentityError error)
        {
            if (
                error.Code ==
                "DuplicateUserName"
            )
            {
                return "UserName";
            }


            if (
                error.Code ==
                "DuplicateEmail"
            )
            {
                return "Email";
            }


            if (
                error.Description.Contains(
                    "Employee Number",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return "EmployeeNumber";
            }


            if (
                error.Description.Contains(
                    "role",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return "Role";
            }


            return string.Empty;
        }


        // =====================================================
        // PASSWORD ERRORS
        // =====================================================

        private static bool IsPasswordError(
            IdentityError error)
        {
            return
                error.Code.StartsWith(
                    "Password",
                    StringComparison.OrdinalIgnoreCase
                );
        }
    }
}