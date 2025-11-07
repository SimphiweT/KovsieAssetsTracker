using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.StyledXmlParser.Jsoup.Nodes;
using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.Account;
using KovsieAssetsTracker.WebAppMVC.Models.Admin;
using KovsieAssetsTracker.WebAppMVC.Models.Asset;
using KovsieAssetsTracker.WebAppMVC.Models.Building;
using KovsieAssetsTracker.WebAppMVC.Models.Campus;
using KovsieAssetsTracker.WebAppMVC.Models.Category;
using KovsieAssetsTracker.WebAppMVC.Models.Floor;
using KovsieAssetsTracker.WebAppMVC.Models.Room;
using KovsieAssetsTracker.WebAppMVC.Models.shared;
using KovsieAssetsTracker.WebAppMVC.Models.User;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Document = iText.Layout.Document;

namespace KovsieAssetsTracker.WebAppMVC.Controllers
{
    public class AdminController : BaseController
    {
        private readonly IAccountService accountService;
        private readonly IUserService userService;
        private readonly IAssetService assetService;
        private readonly ICategoryService categoryService;
        private readonly IRoomService roomService;
        private readonly IFloorService floorService;
        private readonly IBuildingService buildingService;
        private readonly ICampusService campusService;

        public AdminController(IAccountService _accountService, IUserService _userService, IAssetService _assetService, ICategoryService _categoryService, IRoomService _roomService, IFloorService _floorService, IBuildingService _buildingService, ICampusService _campusService, SessionManager _sessionManager) : base(_sessionManager)
        {
            accountService = _accountService;
            userService = _userService;
            assetService = _assetService;
            categoryService = _categoryService;
            roomService = _roomService;
            floorService = _floorService;
            buildingService = _buildingService;
            campusService = _campusService;
        }
        /*====================================================================================================================================
                                                 *** DASHBOARD ***
        ======================================================================================================================================*/
        #region Dashboard
        public async Task<IActionResult> AdminDashboard()
        {
            ForceAdminLayout();
            TempData.SetAlert("info", $"Welcome!... {sessionManager.GetUserDisplayName()}", "Admin Dashboard");


            var assetsTask = assetService.GetAllAssetsAsync();
            var usersTask = userService.GetAllUsersAsync();
            var categoriesTask = categoryService.GetAllCategoriesAsync();
            var roomsTask = roomService.GetAllRoomsAsync();

            await Task.WhenAll(assetsTask, usersTask, categoriesTask, roomsTask);

            var assets = assetsTask.Result?.Data ?? new List<AssetViewModel>();
            var users = usersTask.Result?.Data ?? new List<UserViewModel>();
            var categories = categoriesTask.Result?.Data ?? new List<CategoryViewModel>();
            var rooms = roomsTask.Result?.Data ?? new List<RoomViewModel>();

            // === Basic Stats ===
            int totalAssets = assets.Count;
            int totalUsers = users.Count;
            int totalCategories = categories.Count;
            int totalRooms = rooms.Count;

            // === Top Assets ===
            var topAssets = assets
                .GroupBy(a => a.AssetName)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            // === Assets by Room ===
            var assetsByRoom = assets
                .GroupBy(a => a.RoomName ?? "Unassigned")
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            // === Assets by Category ===
            var assetsByCategory = assets
                .GroupBy(a => a.AssetCategoryName ?? "Uncategorized")
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            // === Assets by Status ===
            var assetsByStatus = assets
                .GroupBy(a => string.IsNullOrEmpty(a.Status) ? "Unknown" : a.Status)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            // === Compose ViewModel ===
            var vm = new DashboardStatsViewModel
            {
                TotalAssets = totalAssets,
                TotalUsers = totalUsers,
                TotalCategories = totalCategories,
                TotalRooms = totalRooms,

                TopAssetNames = topAssets.Select(x => x.Label).ToList(),
                TopAssetCounts = topAssets.Select(x => x.Count).ToList(),

                RoomLabels = assetsByRoom.Select(x => x.Label).ToList(),
                RoomAssetCounts = assetsByRoom.Select(x => x.Count).ToList(),

                CategoryLabels = assetsByCategory.Select(x => x.Label).ToList(),
                CategoryCounts = assetsByCategory.Select(x => x.Count).ToList(),

                StatusLabels = assetsByStatus.Select(x => x.Label).ToList(),
                StatusCounts = assetsByStatus.Select(x => x.Count).ToList()
            };

            return View(vm);
        }
        #endregion

        /*====================================================================================================================================
                                                        *** MANAGE USERS ***
         ======================================================================================================================================*/
        #region Manage Users - Create and List
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            ForceAdminLayout();
            var result = await userService.GetAllUsersAsync();
            var vm = new CreateAndListUserViewModel
            {
                Users = result.Data ?? new List<UserViewModel>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUsers(CreateAndListUserViewModel model)
        {
            ForceAdminLayout();

            if (ModelState.IsValid)
            {
                var result = await userService.RegisterUserAsync(model.CreateUser);

                if (result != null && result.Success)
                {
                    TempData.SetAlert("success", "User created successfully", "Manage Users");
                    return RedirectToAction(nameof(ManageUsers));
                }
                else
                {
        
                        TempData.SetAlert("error", "Failed to create user", "Manage Users");
                        return View(model);
 
                }
            }
            else
                            
            {
                TempData.SetAlert("error", "Failed to create user.", "Manage Users");
                return View(model);
            }

            // repopulate list on failure
            var userResult = await userService.GetAllUsersAsync();
            model.Users = userResult.Data ?? new List<UserViewModel>();
            return View(model);
        }
        #endregion

        /************************************************************* Find And Update User by Id - GET ****************************************************************************/
        #region Find And Update User by Id - GET
        [HttpGet]
        public async Task<IActionResult> ViewAndUpdateUserDetails(string userId)
        {
            ForceAdminLayout();
            if (string.IsNullOrEmpty(userId))
            {
                TempData.SetAlert("error", "User ID missing.", "Manage Users");
                return RedirectToAction(nameof(ManageUsers));
            }
            var result = await userService.GetUserByIdAsync(userId);

            Console.WriteLine($"Fetched user: {result.Data?.UserEmail ?? "null"}");


            if (result == null || !result.Success)
            {
                TempData.SetAlert("error", "User not found.", "Manage Users");
                return RedirectToAction(nameof(ManageUsers));
            }

            var vm = new DetailsAndUpdateUserViewModel
            {
                UserDetails = result.Data,
                UpdateUser = new UpdateUserViewModel
                {
                    UserId = result.Data.UserId,
                 
                    UsernameDisplay = result.Data.UsernameDisplay,
                    UserEmail = result.Data.UserEmail,
                    ContactNumber = result.Data.ContactNumber,
                    StudentStaffNumber = result.Data.StudentStaffNumber,
                    Description = result.Data.Description,
                    UserRole = result.Data.UserRole
                }
            
            };
            TempData["UserId"] = userId;
            return View(vm);
        }


        //[HttpPut]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ViewAndUpdateUserDetails(UpdateUserViewModel model)
        {
            ForceAdminLayout();

            if (string.IsNullOrEmpty(model.UserId) && TempData["UserId"] != null)
            {
                model.UserId = TempData["UserId"].ToString();
            }


            // Always repopulate existing user data
            var existingUser = await userService.GetUserByIdAsync(model.UserId);

            var vm = new DetailsAndUpdateUserViewModel
            {
                UserDetails = existingUser.Data ?? new UserViewModel(),
                UpdateUser = model
            };

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct the highlighted errors.", "Manage Users");
                return View(vm);
            }

            var updateResult = await userService.UpdateUserAsync(model);

            if (updateResult.Success)
            {
                var refreshed = await userService.GetUserByIdAsync(model.UserId);

                var refreshedVm = new DetailsAndUpdateUserViewModel
                {
                    UserDetails = refreshed.Data,
                    UpdateUser = new UpdateUserViewModel
                    {
                        UserId = refreshed.Data.UserId,
                        UsernameDisplay = refreshed.Data.UsernameDisplay,
                        UserEmail = refreshed.Data.UserEmail,
                        ContactNumber = refreshed.Data.ContactNumber,
                        StudentStaffNumber = refreshed.Data.StudentStaffNumber,
                        Description = refreshed.Data.Description,
                        UserRole = refreshed.Data.UserRole
                    }
                };

                TempData.SetAlert("success", "User updated successfully!", "Manage Users");
                return RedirectToAction(nameof(ViewAndUpdateUserDetails), new { userId = model.UserId });
      
            }

            TempData.SetAlert("error", updateResult.Error ?? "Failed to update user.", "Manage Users");
            var current = await userService.GetUserByIdAsync(model.UserId);
            vm.UserDetails = current.Data ?? new UserViewModel();
            return View(vm);

        }
        #endregion

        /************************************************************* Change User Password - GET and POST ****************************************************************************/

        #region Change User Password - GET and POST
        [HttpGet]
        public IActionResult ChangeUserPassword()
        {
            ForceAdminLayout();
            return View();
        }

        [HttpPost]
        public IActionResult ChangeUserPassword(ChangePasswordViewModel changePassword)
        {
            ForceAdminLayout();
            var result = userService.ChangePasswordAsync(changePassword);
            if (result != null)
            {
                // Password change successful
                TempData.SetAlert("success", "Password changed successfully.", "Manage Users");
                return RedirectToAction("ViewUserDetails", new { userId = changePassword.UserId });
            }
            else
            {
                // Password change failed
                TempData.SetAlert("error", "Failed to change password. Please try again.", "Manage Users");
                return RedirectToAction("ViewUserDetails", new { userId = changePassword.UserId });
            }
        }
        #endregion
        /************************************************************* Delete User - POST ****************************************************************************/
        #region Delete User - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            ForceAdminLayout();
            var result = await userService.DeleteUserAsync(userId); // calls API DELETE

            if (result != null && result.Success)
            {
                TempData.SetAlert("success", "User deleted successfully.", "Manage Users");
            }
            else
            {
                TempData.SetAlert("error", "Failed to delete user. Please try again.", "Manage Users");
            }

            return RedirectToAction("ManageUsers");
        }
        #endregion

        /*====================================================================================================================================
                                                         *** MANAGE ASSETS ***
        ======================================================================================================================================*/

        #region Manage Assets
        #region Manage Assets - Create and List
        [HttpGet]
        public async Task<IActionResult> ManageAssets()
        {
            ForceAdminLayout();

            var assetsResult = await assetService.GetAllAssetsAsync();
            var categoriesResult = await categoryService.GetAllCategoriesAsync();
            var roomsResult = await roomService.GetAllRoomsAsync();

            var vm = new CreateAndListAssetViewModel
            {
                Assets = assetsResult.Data ?? new List<AssetViewModel>(),
                CreateAsset = new CreateAssetViewModel(),
                Categories = categoriesResult.Data ?? new List<CategoryViewModel>(),
                Rooms = roomsResult.Data ?? new List<RoomViewModel>()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageAssets(CreateAndListAssetViewModel model)
        {
            ForceAdminLayout();

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("error", "Please fill in all required fields.", "Manage Assets");
                return View(model);
            }

            var result = await assetService.CreateAssetAsync(model.CreateAsset);

            if (result.Success)
            {
                TempData.SetAlert("success", "Asset created successfully!", "Manage Assets");
                return RedirectToAction(nameof(ManageAssets));
            }

            TempData.SetAlert("error", result.Error ?? "Failed to create asset.", "Manage Assets");
            return View(model);
        }
        #endregion

        #region Asset Details and Update

        [HttpGet]
        public async Task<IActionResult> AssetsDetailsAndUpdate(int id)
        {
            ForceAdminLayout();

            var result = await assetService.GetAssetByIdAsync(id);
            var categoriesResult = await categoryService.GetAllCategoriesAsync();
            var roomsResult = await roomService.GetAllRoomsAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                TempData.SetAlert("error", "Asset not found.", "Manage Assets");
                return RedirectToAction(nameof(ManageAssets));
            }

            var vm = new DetailsAndUpdateAssetViewModel
            {
                AssetDetails = result.Data,
                UpdateAsset = new UpdateAssetViewModel
                {
                    AssetServerId = result.Data.AssetServerId,
                    AssetName = result.Data.AssetName,
                    AssetDescription = result.Data.AssetDescription,
                    Status = result.Data.Status,
                    AssetCategoryServerId = result.Data.AssetCategoryServerId,
                    RoomServerId = result.Data.RoomServerId,
                    QRCodeSecretText = result.Data.QRCodeSecretText,
                    IsAndroidBackedUp = result.Data.IsAndroidBackedUp,
                    IsServerBackedUp = result.Data.IsServerBackedUp
                },
                Categories = categoriesResult.Data ?? new List<CategoryViewModel>(),
                Rooms = roomsResult.Data ?? new List<RoomViewModel>()
            };

            TempData["AssetServerId"] = id;
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetsDetailsAndUpdate(UpdateAssetViewModel model)
        {
            ForceAdminLayout();

            if (TempData["AssetServerId"] != null && model.AssetServerId == 0)
                model.AssetServerId = Convert.ToInt32(TempData["AssetServerId"]);

            // Step 1: handle validation error
            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct the highlighted errors before submitting.", "Manage Assets");

                // Reload related data
                var catRes = await categoryService.GetAllCategoriesAsync();
                var roomRes = await roomService.GetAllRoomsAsync();

                // Try to refetch details to display even if validation failed
                var details = await assetService.GetAssetByIdAsync(model.AssetServerId);
                var assetDetails = details.Success && details.Data != null ? details.Data : new AssetViewModel
                {
                    AssetServerId = model.AssetServerId,
                    AssetName = model.AssetName,
                    AssetDescription = model.AssetDescription,
                    Status = model.Status,
                    AssetCategoryServerId = model.AssetCategoryServerId,
                    RoomServerId = model.RoomServerId,
                    QRCodeSecretText = model.QRCodeSecretText,
                    IsAndroidBackedUp = model.IsAndroidBackedUp,
                    IsServerBackedUp = model.IsServerBackedUp
                };

                var vm = new DetailsAndUpdateAssetViewModel
                {
                    AssetDetails = assetDetails,
                    UpdateAsset = model,
                    Categories = catRes.Data ?? new List<CategoryViewModel>(),
                    Rooms = roomRes.Data ?? new List<RoomViewModel>()
                };

                return View(vm);
            }

            // Step 2: process update
            var updateResult = await assetService.UpdateAssetAsync(model);

            if (updateResult.Success)
            {
                TempData.SetAlert("success", "Asset updated successfully!", "Manage Assets");
                return RedirectToAction(nameof(AssetsDetailsAndUpdate), new { id = model.AssetServerId });
            }

            // Step 3: handle failure — re-fetch details to show them even on error
            TempData.SetAlert("error", updateResult.Error ?? "Failed to update asset.", "Manage Assets");

            var categoriesResult = await categoryService.GetAllCategoriesAsync();
            var roomsResult = await roomService.GetAllRoomsAsync();
            var assetResult = await assetService.GetAssetByIdAsync(model.AssetServerId);

            var fallbackDetails = assetResult.Success && assetResult.Data != null ? assetResult.Data : new AssetViewModel
            {
                AssetServerId = model.AssetServerId,
                AssetName = model.AssetName,
                AssetDescription = model.AssetDescription,
                Status = model.Status,
                AssetCategoryServerId = model.AssetCategoryServerId,
                RoomServerId = model.RoomServerId,
                IsAndroidBackedUp = model.IsAndroidBackedUp,
                IsServerBackedUp = model.IsServerBackedUp
            };

            var errorVm = new DetailsAndUpdateAssetViewModel
            {
                AssetDetails = fallbackDetails,
                UpdateAsset = model,
                Categories = categoriesResult.Data ?? new List<CategoryViewModel>(),
                Rooms = roomsResult.Data ?? new List<RoomViewModel>()
            };

            return View(errorVm);
        }

        #endregion


        #region Delete Asset - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsset(int assetId)
        {
            ForceAdminLayout();

            var result = await assetService.DeleteAssetAsync(assetId); // assuming your API supports DELETE

            if (result != null && result.Success)
            {
                TempData.SetAlert("success", "Asset deleted successfully.", "Manage Assets");
            }
            else
            {
                TempData.SetAlert("error", "Failed to delete asset. Please try again.", "Manage Assets");
            }

            return RedirectToAction(nameof(ManageAssets));
        }
        #endregion

        #endregion
        /*====================================================================================================================================
                                                            *** MANAGE CATEGORIES ***
        ======================================================================================================================================*/
        #region Manage Categories
        [HttpGet]
        public async Task<IActionResult> ManageCategories()
        {
            ForceAdminLayout();

            var categoriesResult = await categoryService.GetAllCategoriesWithAssetsAsync();
            var vm = new CreateAndListCategoryViewModel
            {
                CreateCategory = new CreateCategoryViewModel(),
                Categories = categoriesResult.Data?.Select(c => new CategoryViewModel
                {
                    AssetCategoryServerId = c.AssetCategoryServerId,
                    AssetCategoryLocalId = c.AssetCategoryLocalId,
                    AssetCategoryName = c.AssetCategoryName,
                    AssetCategoryDescription = c.AssetCategoryDescription,
                    AssetCategoryImage = c.AssetCategoryImage,
                    IsAndroidBackedUp = c.IsAndroidBackedUp,
                    IsServerBackedUp = c.IsServerBackedUp,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    Assets = c.Assets
                }).ToList() ?? new()
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageCategories(CreateAndListCategoryViewModel model)
        {
            ForceAdminLayout();

            // Validate form
            if (!ModelState.IsValid)
            {
                TempData.SetAlert("error", "Please fill all required fields.", "Manage Categories");

                // Repopulate existing list
                var categoriesResult = await categoryService.GetAllCategoriesWithAssetsAsync();
                model.Categories = categoriesResult.Data ?? new List<CategoryViewModel>();
                return View(model);
            }

            // Try create
            var result = await categoryService.CreateCategoryAsync(model.CreateCategory);

            if (result.Success)
            {
                TempData.SetAlert("success", "Category created successfully!", "Manage Categories");
                return RedirectToAction(nameof(ManageCategories));
            }

            // ❗On failure: repopulate list + keep form values
            var allCategories = await categoryService.GetAllCategoriesWithAssetsAsync();
            model.Categories = allCategories.Data ?? new List<CategoryViewModel>();

            TempData.SetAlert("error", result.Error ?? "Failed to create category.", "Manage Categories");
            return View(model);
        }


        #region Category Details and Update

        [HttpGet]
        public async Task<IActionResult> CategoriesDetailsAndUpdate(int id)
        {
            ForceAdminLayout();

            var result = await categoryService.GetCategoryByIdAsync(id);
            if (result == null || !result.Success || result.Data == null)
            {
                TempData.SetAlert("error", "Category not found.", "Manage Categories");
                return RedirectToAction(nameof(ManageCategories));
            }

            var vm = new DetailsAndUpdateCategoryViewModel
            {
                CategoryDetails = result.Data,
                UpdateCategory = new UpdateCategoryViewModel
                {
                    AssetCategoryServerId = result.Data.AssetCategoryServerId,
                    AssetCategoryName = result.Data.AssetCategoryName,
                    AssetCategoryDescription = result.Data.AssetCategoryDescription,
                    IsAndroidBackedUp = result.Data.IsAndroidBackedUp,
                    IsServerBackedUp = result.Data.IsServerBackedUp
                }
            };

            TempData["AssetCategoryServerId"] = id;
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoriesDetailsAndUpdate(UpdateCategoryViewModel model)
        {
            ForceAdminLayout();

            if (TempData["AssetCategoryServerId"] != null && model.AssetCategoryServerId == 0)
                model.AssetCategoryServerId = Convert.ToInt32(TempData["AssetCategoryServerId"]);

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct highlighted errors.", "Manage Categories");

                var details = await categoryService.GetCategoryByIdAsync(model.AssetCategoryServerId);
                var catDetails = details.Success && details.Data != null ? details.Data : new CategoryViewModel
                {
                    AssetCategoryServerId = model.AssetCategoryServerId,
                    AssetCategoryName = model.AssetCategoryName,
                    AssetCategoryDescription = model.AssetCategoryDescription,
                    IsAndroidBackedUp = model.IsAndroidBackedUp,
                    IsServerBackedUp = model.IsServerBackedUp
                };

                var vm = new DetailsAndUpdateCategoryViewModel
                {
                    CategoryDetails = catDetails,
                    UpdateCategory = model
                };

                return View(vm);
            }

            var updateResult = await categoryService.UpdateCategoryAsync(model);

            if (updateResult.Success)
            {
                TempData.SetAlert("success", "Category updated successfully!", "Manage Categories");
                return RedirectToAction(nameof(CategoriesDetailsAndUpdate), new { id = model.AssetCategoryServerId });
            }

            TempData.SetAlert("error", updateResult.Error ?? "Failed to update category.", "Manage Categories");

            var catRes = await categoryService.GetCategoryByIdAsync(model.AssetCategoryServerId);
            var fallbackDetails = catRes.Success && catRes.Data != null ? catRes.Data : new CategoryViewModel
            {
                AssetCategoryServerId = model.AssetCategoryServerId,
                AssetCategoryName = model.AssetCategoryName,
                AssetCategoryDescription = model.AssetCategoryDescription,
                IsAndroidBackedUp = model.IsAndroidBackedUp,
                IsServerBackedUp = model.IsServerBackedUp
            };

            var errorVm = new DetailsAndUpdateCategoryViewModel
            {
                CategoryDetails = fallbackDetails,
                UpdateCategory = model
            };

            return View(errorVm);
        }

        #endregion


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            ForceAdminLayout();

            var result = await categoryService.DeleteCategoryAsync(categoryId);
            if (result != null && result.Success)
                TempData.SetAlert("success", "Category deleted successfully.", "Manage Categories");
            else
                TempData.SetAlert("error", "Failed to delete category. Please try again.", "Manage Categories");

            return RedirectToAction(nameof(ManageCategories));
        }
        #endregion
        /*====================================================================================================================================
                                                             *** MANAGE CAMPUSES ***
        ======================================================================================================================================*/

        #region Manage Campuses - Create and List

        [HttpGet]
        public async Task<IActionResult> ManageCampuses()
        {
            ForceAdminLayout();

            var campusesResult = await campusService.GetAllCampusesWithBuildingsAsync();
            var vm = new CreateAndListCampusViewModel
            {
                CreateCampus = new CreateCampusViewModel(),
                Campuses = campusesResult.Data ?? new List<CampusViewModel>()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageCampuses(CreateAndListCampusViewModel model)
        {
            ForceAdminLayout();

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("error", "Please fill in all required fields.", "Manage Campuses");

                var campusesResult = await campusService.GetAllCampusesWithBuildingsAsync();
                model.Campuses = campusesResult.Data ?? new List<CampusViewModel>();
                return View(model);
            }

            var result = await campusService.CreateCampusAsync(model.CreateCampus);

            if (result.Success)
            {
                TempData.SetAlert("success", "Campus created successfully!", "Manage Campuses");
                return RedirectToAction(nameof(ManageCampuses));
            }

            var allCampuses = await campusService.GetAllCampusesWithBuildingsAsync();
            model.Campuses = allCampuses.Data ?? new List<CampusViewModel>();

            TempData.SetAlert("error", result.Error ?? "Failed to create campus.", "Manage Campuses");
            return View(model);
        }

        #endregion


        #region Campus Details and Update

        [HttpGet]
        public async Task<IActionResult> CampusDetailsAndUpdate(int id)
        {
            ForceAdminLayout();

            var result = await campusService.GetCampusByIdAsync(id);

            if (result == null || !result.Success || result.Data == null)
            {
                TempData.SetAlert("error", "Campus not found.", "Manage Campuses");
                return RedirectToAction(nameof(ManageCampuses));
            }

            var vm = new DetailsAndUpdateCampusViewModel
            {
                CampusDetails = result.Data,
                UpdateCampus = new UpdateCampusViewModel
                {
                    CampusServerId = result.Data.CampusServerId,
                    CampusName = result.Data.CampusName,
                    CampusDescription = result.Data.CampusDescription,
                    IsAndroidBackedUp = result.Data.IsAndroidBackedUp,
                    IsServerBackedUp = result.Data.IsServerBackedUp
                }
            };

            TempData["CampusServerId"] = id;
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CampusDetailsAndUpdate(UpdateCampusViewModel model)
        {
            ForceAdminLayout();

            if (TempData["CampusServerId"] != null && model.CampusServerId == 0)
                model.CampusServerId = Convert.ToInt32(TempData["CampusServerId"]);

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct highlighted errors before submitting.", "Manage Campuses");

                var details = await campusService.GetCampusByIdAsync(model.CampusServerId);
                var campusDetails = details.Success && details.Data != null ? details.Data : new CampusViewModel
                {
                    CampusServerId = model.CampusServerId,
                    CampusName = model.CampusName,
                    CampusDescription = model.CampusDescription,
                    IsAndroidBackedUp = model.IsAndroidBackedUp,
                    IsServerBackedUp = model.IsServerBackedUp
                };

                var vm = new DetailsAndUpdateCampusViewModel
                {
                    CampusDetails = campusDetails,
                    UpdateCampus = model
                };

                return View(vm);
            }

            var updateResult = await campusService.UpdateCampusAsync(model);

            if (updateResult.Success)
            {
                TempData.SetAlert("success", "Campus updated successfully!", "Manage Campuses");
                return RedirectToAction(nameof(CampusDetailsAndUpdate), new { id = model.CampusServerId });
            }

            TempData.SetAlert("error", updateResult.Error ?? "Failed to update campus.", "Manage Campuses");

            var campusRes = await campusService.GetCampusByIdAsync(model.CampusServerId);
            var fallbackDetails = campusRes.Success && campusRes.Data != null ? campusRes.Data : new CampusViewModel
            {
                CampusServerId = model.CampusServerId,
                CampusName = model.CampusName,
                CampusDescription = model.CampusDescription,
                IsAndroidBackedUp = model.IsAndroidBackedUp,
                IsServerBackedUp = model.IsServerBackedUp
            };

            var errorVm = new DetailsAndUpdateCampusViewModel
            {
                CampusDetails = fallbackDetails,
                UpdateCampus = model
            };

            return View(errorVm);
        }

        #endregion


        #region Delete Campus - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCampus(int campusId)
        {
            ForceAdminLayout();

            var result = await campusService.DeleteCampusAsync(campusId);

            if (result != null && result.Success)
            {
                TempData.SetAlert("success", "Campus deleted successfully.", "Manage Campuses");
            }
            else
            {
                TempData.SetAlert("error", "Failed to delete campus. Please try again.", "Manage Campuses");
            }

            return RedirectToAction(nameof(ManageCampuses));
        }
        #endregion
        /*====================================================================================================================================
        *** PROFILE ***
       ======================================================================================================================================*/
        #region PROFILE

        [HttpGet]
        public async Task<IActionResult> ManageProfile()
        {
            ForceAdminLayout();

            var userId = sessionManager.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                TempData.SetAlert("error", "Session expired. Please log in again.", "Profile");
                return RedirectToAction("Login", "Home");
            }

            var result = await userService.GetUserByIdAsync(userId);
            if (result == null || !result.Success || result.Data == null)
            {
                TempData.SetAlert("error", "Unable to load your profile.", "Profile");
                return RedirectToAction(nameof(AdminDashboard));
            }

            var vm = new DetailsAndUpdateUserViewModel
            {
                UserDetails = result.Data,
                UpdateUser = new UpdateUserViewModel
                {
                    UserId = result.Data.UserId!,
                    UsernameDisplay = result.Data.UsernameDisplay,
                    UserEmail = result.Data.UserEmail,
                    StudentStaffNumber = result.Data.StudentStaffNumber,
                    ContactNumber = result.Data.ContactNumber,
                    Description = result.Data.Description,
                    UserRole = result.Data.UserRole
                }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageProfile(UpdateUserViewModel model)
        {
            ForceAdminLayout();

            // Always ensure the UserId is present
            if (string.IsNullOrWhiteSpace(model.UserId))
                model.UserId = sessionManager.GetUserId() ?? string.Empty;

            if (!ModelState.IsValid)
            {
                // Re-fetch details so the left card is filled
                var details = await userService.GetUserByIdAsync(model.UserId);
                var vm = new DetailsAndUpdateUserViewModel
                {
                    UserDetails = details.Success ? details.Data : new UserViewModel
                    {
                        UserId = model.UserId,
                        UsernameDisplay = model.UsernameDisplay,
                        UserEmail = model.UserEmail,
                        StudentStaffNumber = model.StudentStaffNumber,
                        ContactNumber = model.ContactNumber,
                        Description = model.Description,
                        UserRole = sessionManager.GetRole()
                    },
                    UpdateUser = model
                };

                TempData.SetAlert("warning", "Please correct highlighted errors.", "Profile");
                return View(vm);
            }

            var result = await userService.UpdateUserAsync(model);

            if (result.Success)
            {
                TempData.SetAlert("success", "Profile updated successfully!", "Profile");

                // keep header/avatar in sync
                sessionManager.SetUserDisplayName(model.UsernameDisplay ?? "");
                if (!string.IsNullOrWhiteSpace(result.Data?.ProfileImageFileName))
                    sessionManager.SetProfilePictureUrl(result.Data.ProfileImageFileName);

                return RedirectToAction(nameof(ManageProfile));
            }

            // ⬇️ On API error: still show details on the left
            var current = await userService.GetUserByIdAsync(model.UserId);
            var backVm = new DetailsAndUpdateUserViewModel
            {
                UserDetails = current.Success ? current.Data : new UserViewModel
                {
                    UserId = model.UserId,
                    UsernameDisplay = model.UsernameDisplay,
                    UserEmail = model.UserEmail,
                    StudentStaffNumber = model.StudentStaffNumber,
                    ContactNumber = model.ContactNumber,
                    Description = model.Description,
                    UserRole = sessionManager.GetRole()
                },
                UpdateUser = model
            };

            TempData.SetAlert("error", result.Error ?? "Failed to update profile.", "Profile");
            return View(backVm);
        }



        #endregion

        /*====================================================================================================================================
                                                      *** MANAGE BUILDINGS ***
=====================================================================================================================================*/
        #region Manage Buildings - Create, List, Update, Delete

        [HttpGet]
        public async Task<IActionResult> ManageBuildings()
        {
            ForceAdminLayout();

            var buildingsResult = await buildingService.GetAllBuildingsWithFloorsAsync();
            var campusesResult = await campusService.GetAllCampusesWithBuildingsAsync();

            var vm = new CreateAndListBuildingsViewModel
            {
                CreateBuilding = new CreateBuildingViewModel(),
                Buildings = buildingsResult.Data ?? new List<BuildingViewModel>(),
                Campuses = campusesResult.Data ?? new List<CampusViewModel>()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageBuildings(CreateAndListBuildingsViewModel model)
        {
            ForceAdminLayout();

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("error", "Please fill in all required fields.", "Manage Buildings");
                var buildingsResult = await buildingService.GetAllBuildingsWithFloorsAsync();
                var campusesResult = await campusService.GetAllCampusesWithBuildingsAsync();
                model.Buildings = buildingsResult.Data ?? new List<BuildingViewModel>();
                model.Campuses = campusesResult.Data ?? new List<CampusViewModel>();
                return View(model);
            }

            var result = await buildingService.CreateBuildingAsync(model.CreateBuilding);

            if (result.Success)
            {
                TempData.SetAlert("success", "Building created successfully!", "Manage Buildings");
                return RedirectToAction(nameof(ManageBuildings));
            }

            TempData.SetAlert("error", result.Error ?? "Failed to create building.", "Manage Buildings");
            return RedirectToAction(nameof(ManageBuildings));
        }

        #region Building Details and Update

        [HttpGet]
        public async Task<IActionResult> BuildingDetailsAndUpdate(int id)
        {
            ForceAdminLayout();

            var result = await buildingService.GetBuildingByIdAsync(id);
            var campusResult = await campusService.GetAllCampusesAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                TempData.SetAlert("error", "Building not found.", "Manage Buildings");
                return RedirectToAction(nameof(ManageBuildings));
            }

            var vm = new DetailsAndUpdateBuildingViewModel
            {
                BuildingDetails = result.Data,
                UpdateBuilding = new UpdateBuildingViewModel
                {
                    BuildingServerId = result.Data.BuildingServerId,
                    BuildingName = result.Data.BuildingName,
                    BuildingDescription = result.Data.BuildingDescription,
                    CampusServerId = result.Data.CampusServerId,
                    IsAndroidBackedUp = result.Data.IsAndroidBackedUp,
                    IsServerBackedUp = result.Data.IsServerBackedUp
                },
                Campuses = campusResult.Data ?? new List<CampusViewModel>()
            };

            TempData["BuildingServerId"] = id;
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuildingDetailsAndUpdate(UpdateBuildingViewModel model)
        {
            ForceAdminLayout();

            if (TempData["BuildingServerId"] != null && model.BuildingServerId == 0)
                model.BuildingServerId = Convert.ToInt32(TempData["BuildingServerId"]);

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct the highlighted errors.", "Manage Buildings");

                var campusResult = await campusService.GetAllCampusesAsync();
                var details = await buildingService.GetBuildingByIdAsync(model.BuildingServerId);
                var buildingDetails = details.Success && details.Data != null ? details.Data : new BuildingViewModel
                {
                    BuildingServerId = model.BuildingServerId,
                    BuildingName = model.BuildingName,
                    BuildingDescription = model.BuildingDescription,
                    CampusServerId = model.CampusServerId,
                    IsAndroidBackedUp = model.IsAndroidBackedUp,
                    IsServerBackedUp = model.IsServerBackedUp
                };

                var vm = new DetailsAndUpdateBuildingViewModel
                {
                    BuildingDetails = buildingDetails,
                    UpdateBuilding = model,
                    Campuses = campusResult.Data ?? new List<CampusViewModel>()
                };

                return View(vm);
            }

            var updateResult = await buildingService.UpdateBuildingAsync(model);

            if (updateResult.Success)
            {
                TempData.SetAlert("success", "Building updated successfully!", "Manage Buildings");
                return RedirectToAction(nameof(BuildingDetailsAndUpdate), new { id = model.BuildingServerId });
            }

            TempData.SetAlert("error", updateResult.Error ?? "Failed to update building.", "Manage Buildings");

            var campusRes = await campusService.GetAllCampusesAsync();
            var buildingRes = await buildingService.GetBuildingByIdAsync(model.BuildingServerId);
            var fallbackDetails = buildingRes.Success && buildingRes.Data != null ? buildingRes.Data : new BuildingViewModel
            {
                BuildingServerId = model.BuildingServerId,
                BuildingName = model.BuildingName,
                BuildingDescription = model.BuildingDescription,
                CampusServerId = model.CampusServerId,
                IsAndroidBackedUp = model.IsAndroidBackedUp,
                IsServerBackedUp = model.IsServerBackedUp
            };

            var errorVm = new DetailsAndUpdateBuildingViewModel
            {
                BuildingDetails = fallbackDetails,
                UpdateBuilding = model,
                Campuses = campusRes.Data ?? new List<CampusViewModel>()
            };

            return View(errorVm);
        }

        #endregion


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBuilding(int buildingId)
        {
            ForceAdminLayout();

            var result = await buildingService.DeleteBuildingAsync(buildingId);
            if (result != null && result.Success)
                TempData.SetAlert("success", "Building deleted successfully.", "Manage Buildings");
            else
                TempData.SetAlert("error", "Failed to delete building. Please try again.", "Manage Buildings");

            return RedirectToAction(nameof(ManageBuildings));
        }
        #endregion

        /*====================================================================================================================================
                                                    *** MANAGE FLOORS ***
=====================================================================================================================================*/
        #region Manage Floors - Create, List, Update, Delete

        [HttpGet]
        public async Task<IActionResult> ManageFloors()
        {
            ForceAdminLayout();

            var floorsResult = await floorService.GetAllFloorsWithRoomsAsync();
            var buildingsResult = await buildingService.GetAllBuildingsWithFloorsAsync();

            var vm = new CreateAndListFloorsViewModel
            {
                CreateFloor = new CreateFloorViewModel(),
                Floors = floorsResult.Data ?? new List<FloorViewModel>(),
                Buildings = buildingsResult.Data ?? new List<BuildingViewModel>()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageFloors(CreateAndListFloorsViewModel model)
        {
            ForceAdminLayout();

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("error", "Please fill all required fields.", "Manage Floors");
                return RedirectToAction(nameof(ManageFloors));
            }

            var result = await floorService.CreateFloorAsync(model.CreateFloor);
            if (result.Success)
                TempData.SetAlert("success", "Floor created successfully!", "Manage Floors");
            else
                TempData.SetAlert("error", result.Error ?? "Failed to create floor.", "Manage Floors");

            return RedirectToAction(nameof(ManageFloors));
        }

        #region Floor Details and Update

        [HttpGet]
        public async Task<IActionResult> FloorDetailsAndUpdate(int id)
        {
            ForceAdminLayout();

            var result = await floorService.GetFloorByIdAsync(id);
            var buildingResult = await buildingService.GetAllBuildingsAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                TempData.SetAlert("error", "Floor not found.", "Manage Floors");
                return RedirectToAction(nameof(ManageFloors));
            }

            var vm = new DetailsAndUpdateFloorViewModel
            {
                FloorDetails = result.Data,
                UpdateFloor = new UpdateFloorViewModel
                {
                    FloorServerId = result.Data.FloorServerId,
                    FloorName = result.Data.FloorName,
                    FloorDescription = result.Data.FloorDescription,
                    BuildingServerId = result.Data.BuildingServerId
                },
                Buildings = buildingResult.Data ?? new List<BuildingViewModel>()
            };

            TempData["FloorServerId"] = id;
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FloorDetailsAndUpdate(UpdateFloorViewModel model)
        {
            ForceAdminLayout();

            if (TempData["FloorServerId"] != null && model.FloorServerId == 0)
                model.FloorServerId = Convert.ToInt32(TempData["FloorServerId"]);

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct highlighted errors.", "Manage Floors");

                var buildingResult = await buildingService.GetAllBuildingsAsync();
                var details = await floorService.GetFloorByIdAsync(model.FloorServerId);
                var floorDetails = details.Success && details.Data != null ? details.Data : new FloorViewModel
                {
                    FloorServerId = model.FloorServerId,
                    FloorName = model.FloorName,
                    FloorDescription = model.FloorDescription,
                    BuildingServerId = model.BuildingServerId
                };

                var vm = new DetailsAndUpdateFloorViewModel
                {
                    FloorDetails = floorDetails,
                    UpdateFloor = model,
                    Buildings = buildingResult.Data ?? new List<BuildingViewModel>()
                };

                return View(vm);
            }

            var updateResult = await floorService.UpdateFloorAsync(model);

            if (updateResult.Success)
            {
                TempData.SetAlert("success", "Floor updated successfully!", "Manage Floors");
                return RedirectToAction(nameof(FloorDetailsAndUpdate), new { id = model.FloorServerId });
            }

            TempData.SetAlert("error", updateResult.Error ?? "Failed to update floor.", "Manage Floors");

            var buildRes = await buildingService.GetAllBuildingsAsync();
            var floorRes = await floorService.GetFloorByIdAsync(model.FloorServerId);
            var fallbackDetails = floorRes.Success && floorRes.Data != null ? floorRes.Data : new FloorViewModel
            {
                FloorServerId = model.FloorServerId,
                FloorName = model.FloorName,
                FloorDescription = model.FloorDescription,
                BuildingServerId = model.BuildingServerId
            };

            var errorVm = new DetailsAndUpdateFloorViewModel
            {
                FloorDetails = fallbackDetails,
                UpdateFloor = model,
                Buildings = buildRes.Data ?? new List<BuildingViewModel>()
            };

            return View(errorVm);
        }

        #endregion


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFloor(int floorId)
        {
            ForceAdminLayout();

            var result = await floorService.DeleteFloorAsync(floorId);
            if (result != null && result.Success)
                TempData.SetAlert("success", "Floor deleted successfully.", "Manage Floors");
            else
                TempData.SetAlert("error", "Failed to delete floor.", "Manage Floors");

            return RedirectToAction(nameof(ManageFloors));
        }
        #endregion
        /*====================================================================================================================================
                                                                 *** MANAGE ROOMS ***
        ======================================================================================================================================*/
        #region Manage Rooms
        [HttpGet]
        public async Task<IActionResult> ManageRooms()
        {
            ForceAdminLayout();

            var roomsResult = await roomService.GetAllRoomsAsync();
            var floorsResult = await floorService.GetAllFloorsAsync();

            var vm = new CreateAndListRoomsViewModel
            {
                CreateRoom = new CreateRoomViewModel(),
                Rooms = roomsResult.Data ?? new List<RoomViewModel>(),
                Floors = floorsResult.Data ?? new List<FloorViewModel>()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRooms(CreateAndListRoomsViewModel model)
        {
            ForceAdminLayout();

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("error", "Please fill all required fields.", "Manage Rooms");
                var roomsResult = await roomService.GetAllRoomsAsync();
                var floorsResult = await floorService.GetAllFloorsAsync();
                model.Rooms = roomsResult.Data ?? new List<RoomViewModel>();
                model.Floors = floorsResult.Data ?? new List<FloorViewModel>();
                return View(model);
            }

            var result = await roomService.CreateRoomAsync(model.CreateRoom);

            if (result.Success)
            {
                TempData.SetAlert("success", "Room created successfully!", "Manage Rooms");
                return RedirectToAction(nameof(ManageRooms));
            }

            var allRooms = await roomService.GetAllRoomsAsync();
            var allFloors = await floorService.GetAllFloorsAsync();
            model.Rooms = allRooms.Data ?? new List<RoomViewModel>();
            model.Floors = allFloors.Data ?? new List<FloorViewModel>();

            TempData.SetAlert("error", result.Error ?? "Failed to create room.", "Manage Rooms");
            return View(model);
        }

        #region Room Details and Update

        [HttpGet]
        public async Task<IActionResult> RoomsDetailsAndUpdate(int id)
        {
            ForceAdminLayout();

            var result = await roomService.GetRoomByIdAsync(id);
            var floorsResult = await floorService.GetAllFloorsAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                TempData.SetAlert("error", "Room not found.", "Manage Rooms");
                return RedirectToAction(nameof(ManageRooms));
            }

            var vm = new DetailsAndUpdateRoomViewModel
            {
                RoomDetails = result.Data,
                UpdateRoom = new UpdateRoomViewModel
                {
                    RoomServerId = result.Data.RoomServerId,
                    RoomName = result.Data.RoomName,
                    RoomDescription = result.Data.RoomDescription,
                    FloorServerId = result.Data.FloorServerId,
                    IsAndroidBackedUp = result.Data.IsAndroidBackedUp,
                    IsServerBackedUp = result.Data.IsServerBackedUp
                },
                Floors = floorsResult.Data ?? new List<FloorViewModel>()
            };

            TempData["RoomServerId"] = id;
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoomsDetailsAndUpdate(UpdateRoomViewModel model)
        {
            ForceAdminLayout();

            if (TempData["RoomServerId"] != null && model.RoomServerId == 0)
                model.RoomServerId = Convert.ToInt32(TempData["RoomServerId"]);

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct highlighted errors.", "Manage Rooms");

                var floorsResult = await floorService.GetAllFloorsAsync();
                var details = await roomService.GetRoomByIdAsync(model.RoomServerId);
                var roomDetails = details.Success && details.Data != null ? details.Data : new RoomViewModel
                {
                    RoomServerId = model.RoomServerId,
                    RoomName = model.RoomName,
                    RoomDescription = model.RoomDescription,
                    FloorServerId = model.FloorServerId,
                    IsAndroidBackedUp = model.IsAndroidBackedUp,
                    IsServerBackedUp = model.IsServerBackedUp
                };

                var vm = new DetailsAndUpdateRoomViewModel
                {
                    RoomDetails = roomDetails,
                    UpdateRoom = model,
                    Floors = floorsResult.Data ?? new List<FloorViewModel>()
                };

                return View(vm);
            }

            var updateResult = await roomService.UpdateRoomAsync(model);

            if (updateResult.Success)
            {
                TempData.SetAlert("success", "Room updated successfully!", "Manage Rooms");
                return RedirectToAction(nameof(RoomsDetailsAndUpdate), new { id = model.RoomServerId });
            }

            TempData.SetAlert("error", updateResult.Error ?? "Failed to update room.", "Manage Rooms");

            var floorRes = await floorService.GetAllFloorsAsync();
            var roomRes = await roomService.GetRoomByIdAsync(model.RoomServerId);
            var fallbackDetails = roomRes.Success && roomRes.Data != null ? roomRes.Data : new RoomViewModel
            {
                RoomServerId = model.RoomServerId,
                RoomName = model.RoomName,
                RoomDescription = model.RoomDescription,
                FloorServerId = model.FloorServerId,
                IsAndroidBackedUp = model.IsAndroidBackedUp,
                IsServerBackedUp = model.IsServerBackedUp
            };

            var errorVm = new DetailsAndUpdateRoomViewModel
            {
                RoomDetails = fallbackDetails,
                UpdateRoom = model,
                Floors = floorRes.Data ?? new List<FloorViewModel>()
            };

            return View(errorVm);
        }

        #endregion


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoom(int roomId)
        {
            ForceAdminLayout();

            var result = await roomService.DeleteRoomAsync(roomId);
            if (result != null && result.Success)
                TempData.SetAlert("success", "Room deleted successfully.", "Manage Rooms");
            else
                TempData.SetAlert("error", "Failed to delete room.", "Manage Rooms");

            return RedirectToAction(nameof(ManageRooms));
        }
        #endregion
        /*====================================================================================================================================
                                                             *** MANAGE PASSWORDS ***
        ======================================================================================================================================*/

        #region CHANGE PASSWORD

        [HttpGet]
        public async Task<IActionResult> ManagePasswords()
        {
            ForceAdminLayout();

            var vm = new ChangePasswordViewModel
            {
                UserId = sessionManager.GetUserId()
            };

            var users = await userService.GetAllUsersAsync();
            ViewBag.Users = users?.Data ?? new List<UserViewModel>();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePasswords(ChangePasswordViewModel model)
        {
            ForceAdminLayout();

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct the highlighted errors.", "Change Password");
                return View(model);
            }

            var result = await userService.ChangePasswordAsync(model);

            if (result.Success)
            {
                TempData.SetAlert("success", "Password changed successfully! Please log in again.", "Account");
                await accountService.LogoutAsync(); // clear session if needed
                return RedirectToAction("Login", "Home");
            }

            TempData.SetAlert("error", result.Error ?? "Failed to change password. Please try again.", "Account");
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(string email, string temporaryPassword)
        {
            ForceAdminLayout();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(temporaryPassword))
            {
                TempData.SetAlert("warning", "Please fill in all required fields.", "Reset Password");
                return RedirectToAction("ManagePasswords");
            }

            var result = await userService.AdminResetPasswordAsync(email, temporaryPassword);

            if (result.Success)
            {
                TempData.SetAlert("success", "User password reset successfully. An email notification has been sent.", "Reset Password");
            }
            else
            {
                TempData.SetAlert("error", result.Error ?? "Failed to reset user password.", "Reset Password");
            }

            return RedirectToAction("ManagePasswords");
        }

        #endregion
        /*====================================================================================================================================
                                                     *** MANAGE ACCESS ***
======================================================================================================================================*/
        #region Manage Access Requests

        [HttpGet]
        public async Task<IActionResult> ManageAccessRequests()
        {
            ForceAdminLayout();
            var result = await accountService.GetAllAccessRequestsAsync();

            if (!result.Success)
            {
                TempData.SetAlert("error", result.Error ?? "Unable to fetch access requests.", "Access Requests");
                return View(new List<AccessResponseViewModel>());
            }

            var requests = result.Data ?? new List<AccessResponseViewModel>();
            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAccessRequest(int id)
        {
            ForceAdminLayout();
            var result = await accountService.ApproveAccessRequestAsync(id);
            if (result.Success)
                TempData.SetAlert("success", "Access request approved successfully!", "Access Requests");
            else
                TempData.SetAlert("error", result.Error ?? "Failed to approve request.", "Access Requests");

            return RedirectToAction(nameof(ManageAccessRequests));
        }

        [HttpPost]
        public async Task<IActionResult> DeclineAccessRequest(int id, string reason)
        {
            ForceAdminLayout();
            var result = await accountService.DeclineAccessRequestAsync(id, reason);
            if (result.Success)
                TempData.SetAlert("success", "Access request declined successfully.", "Access Requests");
            else
                TempData.SetAlert("error", result.Error ?? "Failed to decline request.", "Access Requests");

            return RedirectToAction(nameof(ManageAccessRequests));
        }

        #endregion


        /*====================================================================================================================================
                                                         *** EXPORT / SIGN OUT ***
         ======================================================================================================================================*/
        #region Export / Sign Out

        public IActionResult LogOut()
        {
            ForceAdminLayout();
            sessionManager.Clear();

            TempData.SetAlert("info", "You have been signed out.", "Session Ended");
            return RedirectToAction("Login", "Home");
        }


        [HttpGet]
        public async Task<IActionResult> ExportUsers()
        {
            var result = await userService.GetAllUsersAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No users available to export.", "Export Users");
                return RedirectToAction(nameof(ManageUsers));
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users Report");

            // Header
            worksheet.Cell(1, 1).Value = "User ID";
            worksheet.Cell(1, 2).Value = "Username";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Role";
            worksheet.Cell(1, 5).Value = "Contact";
            worksheet.Cell(1, 6).Value = "Student/Staff No";
            worksheet.Cell(1, 7).Value = "Created";
            worksheet.Cell(1, 8).Value = "Updated";

            // Data
            int row = 2;
            foreach (var u in result.Data)
            {
                worksheet.Cell(row, 1).Value = u.UserId;
                worksheet.Cell(row, 2).Value = u.UsernameDisplay;
                worksheet.Cell(row, 3).Value = u.UserEmail;
                worksheet.Cell(row, 4).Value = u.UserRole;
                worksheet.Cell(row, 5).Value = u.ContactNumber;
                worksheet.Cell(row, 6).Value = u.StudentStaffNumber;
                worksheet.Cell(row, 7).Value = u.CreatedDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 8).Value = u.UpdatedDate?.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UsersReport.xlsx");
        }


        [HttpGet]
        public async Task<IActionResult> ExportUsersPdf()
        {
            var result = await userService.GetAllUsersAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No users available to export.", "Export Users");
                return RedirectToAction(nameof(ManageUsers));
            }

            using var stream = new MemoryStream();
            using (var writer = new PdfWriter(stream))        
            using (var pdf = new PdfDocument(writer))
            {
                var document = new Document(pdf);

                // --- Header Logo ---
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = new Image(ImageDataFactory.Create(logoPath))
                        .ScaleAbsolute(120, 40)
                        .SetFixedPosition(40, pdf.GetDefaultPageSize().GetTop() - 60);
                    document.Add(logo);
                }

                // --- Title ---
                var title = new Paragraph("Users Report")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(18)
                    .SetFontColor(ColorConstants.BLACK)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(50)
                    .SetMarginBottom(20);
                document.Add(title);

                // --- Table ---
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 20, 20, 20, 20, 20 })).UseAllAvailableWidth();
                string[] headers = { "Username", "Email", "Role", "Contact", "Staff/Student #" };
                foreach (var header in headers)
                {
                    table.AddHeaderCell(new Cell()
                        .Add(new Paragraph(header)
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                        .SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(new DeviceRgb(33, 150, 243))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetPadding(5));
                }

                foreach (var u in result.Data)
                {
                    table.AddCell(new Cell().Add(new Paragraph(u.UsernameDisplay ?? "-")).SetTextAlignment(TextAlignment.LEFT));
                    table.AddCell(new Cell().Add(new Paragraph(u.UserEmail ?? "-")).SetTextAlignment(TextAlignment.LEFT));
                    table.AddCell(new Cell().Add(new Paragraph(u.UserRole ?? "-")).SetTextAlignment(TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(u.ContactNumber ?? "-")).SetTextAlignment(TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(u.StudentStaffNumber ?? "-")).SetTextAlignment(TextAlignment.CENTER));
                }

                document.Add(table);

                // --- Footer ---
                var footer = new Paragraph($"\nGenerated by KovsieAssets Tracker on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(30);
                document.Add(footer);

                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"UsersReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        #endregion
        /*====================================================================================================================================
                                 *** EXPORT ASSETS  ***
 =====================================================================================================================================*/
        #region Export Assets
        [HttpGet]
        public async Task<IActionResult> ExportAssetsExcel()
        {
            ForceAdminLayout();

            var result = await assetService.GetAllAssetsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No assets available to export.", "Export Assets");
                return RedirectToAction(nameof(ManageAssets));
            }

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Assets Report");

            // Headers
            worksheet.Cell(1, 1).Value = "Asset ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Category";
            worksheet.Cell(1, 5).Value = "Room";
            worksheet.Cell(1, 6).Value = "Status";
            worksheet.Cell(1, 7).Value = "Created";
            worksheet.Cell(1, 8).Value = "Updated";

            // Data
            int row = 2;
            foreach (var a in result.Data)
            {
                worksheet.Cell(row, 1).Value = a.AssetServerId;
                worksheet.Cell(row, 2).Value = a.AssetName;
                worksheet.Cell(row, 3).Value = a.AssetDescription;
                worksheet.Cell(row, 4).Value = a.AssetCategoryName;
                worksheet.Cell(row, 5).Value = a.RoomName;
                worksheet.Cell(row, 6).Value = a.Status;
                worksheet.Cell(row, 7).Value = a.CreatedDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 8).Value = a.UpdatedDate?.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AssetsReport.xlsx");
        }
        [HttpGet]
        public async Task<IActionResult> ExportAssetsPdf()
        {
            ForceAdminLayout();

            var result = await assetService.GetAllAssetsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No assets available to export.", "Export Assets");
                return RedirectToAction(nameof(ManageAssets));
            }

            using var stream = new MemoryStream();
            using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf);

                // Header Logo
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(logoPath))
                        .ScaleAbsolute(120, 40)
                        .SetFixedPosition(40, pdf.GetDefaultPageSize().GetTop() - 60);
                    document.Add(logo);
                }

                // Title
                var title = new iText.Layout.Element.Paragraph("Assets Report")
                    .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(18)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginTop(50)
                    .SetMarginBottom(20);
                document.Add(title);

                // Table
                var table = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 10, 20, 20, 15, 15, 10 })).UseAllAvailableWidth();
                string[] headers = { "ID", "Name", "Description", "Category", "Room", "Status" };
                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(header)
                        .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                        .SetBackgroundColor(new iText.Kernel.Colors.DeviceRgb(33, 150, 243))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5));
                }

                foreach (var a in result.Data)
                {
                    table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(a.AssetServerId.ToString())));
                    table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(a.AssetName ?? "-")));
                    table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(a.AssetDescription ?? "-")));
                    table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(a.AssetCategoryName ?? "-")));
                    table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(a.RoomName ?? "-")));
                    table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(a.Status ?? "-")));
                }

                document.Add(table);

                // Footer
                var footer = new iText.Layout.Element.Paragraph($"\nGenerated by KovsieAssets Tracker on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_OBLIQUE))
                    .SetFontSize(10)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginTop(30);
                document.Add(footer);

                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"AssetsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        #endregion
        /*====================================================================================================================================
                                       *** EXPORT CATEGORY  ***
       =====================================================================================================================================*/
        #region Export Categories

        [HttpGet]
        public async Task<IActionResult> ExportCategoriesExcel()
        {
            ForceAdminLayout();

            var result = await categoryService.GetAllCategoriesWithAssetsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No categories available to export.", "Export Categories");
                return RedirectToAction(nameof(ManageCategories));
            }

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Categories Report");

            // Header row
            worksheet.Cell(1, 1).Value = "Category ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Asset Count";
            worksheet.Cell(1, 5).Value = "Created Date";
            worksheet.Cell(1, 6).Value = "Updated Date";

            int row = 2;
            foreach (var c in result.Data)
            {
                worksheet.Cell(row, 1).Value = c.AssetCategoryServerId;
                worksheet.Cell(row, 2).Value = c.AssetCategoryName;
                worksheet.Cell(row, 3).Value = c.AssetCategoryDescription;
                worksheet.Cell(row, 4).Value = c.Assets?.Count ?? 0;
                worksheet.Cell(row, 5).Value = c.CreatedDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 6).Value = c.UpdatedDate?.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"CategoriesReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportCategoriesPdf()
        {
            ForceAdminLayout();

            var result = await categoryService.GetAllCategoriesWithAssetsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No categories available to export.", "Export Categories");
                return RedirectToAction(nameof(ManageCategories));
            }

            using var stream = new MemoryStream();
            using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf);

                var fontRegular = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var fontBold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                var blue = new iText.Kernel.Colors.DeviceRgb(40, 123, 255);
                var peach = new iText.Kernel.Colors.DeviceRgb(255, 182, 123);
                var white = iText.Kernel.Colors.ColorConstants.WHITE;

                // Header
                var pageSize = pdf.GetDefaultPageSize();
                var width = pageSize.GetWidth();
                var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(pdf.AddNewPage());
                canvas.SetFillColor(blue)
                      .Rectangle(0, pageSize.GetTop() - 60, width, 60)
                      .Fill();

                var title = new iText.Layout.Element.Paragraph("KOVSIE ASSETS TRACKER - CATEGORY REPORT")
                    .SetFont(fontBold)
                    .SetFontSize(16)
                    .SetFontColor(white)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFixedPosition(0, pageSize.GetTop() - 45, width);
                document.Add(title);

                document.Add(new iText.Layout.Element.Paragraph("\n\n\n"));

                // Table
                var table = new iText.Layout.Element.Table(new float[] { 2, 3, 4, 2, 2 }).UseAllAvailableWidth();
                string[] headers = { "ID", "Name", "Description", "Assets", "Created" };

                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(header).SetFont(fontBold).SetFontColor(white))
                        .SetBackgroundColor(blue)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5));
                }

                foreach (var c in result.Data)
                {
                    table.AddCell(c.AssetCategoryServerId.ToString());
                    table.AddCell(c.AssetCategoryName ?? "-");
                    table.AddCell(c.AssetCategoryDescription ?? "-");
                    table.AddCell((c.Assets?.Count ?? 0).ToString());
                    table.AddCell(c.CreatedDate?.ToString("yyyy-MM-dd") ?? "-");
                }

                document.Add(table);

                document.Add(new iText.Layout.Element.Paragraph(
                    $"\nReport generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(fontRegular)
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY));

                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"CategoriesReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        #endregion
        /*====================================================================================================================================
                                    *** EXPORT ROOMS  ***
    =====================================================================================================================================*/
        #region Export Rooms

        [HttpGet]
        public async Task<IActionResult> ExportRoomsExcel()
        {
            ForceAdminLayout();

            var result = await roomService.GetAllRoomsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No rooms available to export.", "Export Rooms");
                return RedirectToAction(nameof(ManageRooms));
            }

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Rooms Report");

            worksheet.Cell(1, 1).Value = "Room ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Floor";
            worksheet.Cell(1, 5).Value = "Asset Count";
            worksheet.Cell(1, 6).Value = "Created";
            worksheet.Cell(1, 7).Value = "Updated";

            int row = 2;
            foreach (var r in result.Data)
            {
                worksheet.Cell(row, 1).Value = r.RoomServerId;
                worksheet.Cell(row, 2).Value = r.RoomName;
                worksheet.Cell(row, 3).Value = r.RoomDescription;
                worksheet.Cell(row, 4).Value = r.FloorName ?? "-";
                worksheet.Cell(row, 5).Value = r.Assets?.Count ?? 0;
                worksheet.Cell(row, 6).Value = r.CreatedDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 7).Value = r.UpdatedDate?.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"RoomsReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportRoomsPdf()
        {
            ForceAdminLayout();

            var result = await roomService.GetAllRoomsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No rooms available to export.", "Export Rooms");
                return RedirectToAction(nameof(ManageRooms));
            }

            using var stream = new MemoryStream();
            using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf);
                var fontRegular = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var fontBold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var blue = new iText.Kernel.Colors.DeviceRgb(40, 123, 255);

                document.Add(new iText.Layout.Element.Paragraph("KOVSIE ASSETS TRACKER - ROOMS REPORT")
                    .SetFont(fontBold)
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontColor(blue)
                    .SetMarginBottom(15));

                var table = new iText.Layout.Element.Table(new float[] { 2, 3, 4, 2, 2 }).UseAllAvailableWidth();
                string[] headers = { "ID", "Room Name", "Description", "Floor", "Assets" };

                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(header).SetFont(fontBold).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                        .SetBackgroundColor(blue)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5));
                }

                foreach (var r in result.Data)
                {
                    table.AddCell(r.RoomServerId.ToString());
                    table.AddCell(r.RoomName ?? "-");
                    table.AddCell(r.RoomDescription ?? "-");
                    table.AddCell(r.FloorName ?? "-");
                    table.AddCell((r.Assets?.Count ?? 0).ToString());
                }

                document.Add(table);

                document.Add(new iText.Layout.Element.Paragraph($"\nGenerated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(fontRegular)
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY));

                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"RoomsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        #endregion
        /*====================================================================================================================================
                                      *** EXPORT CAMPUSES  ***
      =====================================================================================================================================*/
        #region Export Campuses
        [HttpGet]
        public async Task<IActionResult> ExportCampusesExcel()
        {
            ForceAdminLayout();

            var result = await campusService.GetAllCampusesWithBuildingsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No campuses available to export.", "Export Campuses");
                return RedirectToAction(nameof(ManageCampuses));
            }

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Campuses Report");

            worksheet.Cell(1, 1).Value = "Campus ID";
            worksheet.Cell(1, 2).Value = "Campus Name";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Building Count";
            worksheet.Cell(1, 5).Value = "Created";
            worksheet.Cell(1, 6).Value = "Updated";

            int row = 2;
            foreach (var c in result.Data)
            {
                worksheet.Cell(row, 1).Value = c.CampusServerId;
                worksheet.Cell(row, 2).Value = c.CampusName;
                worksheet.Cell(row, 3).Value = c.CampusDescription;
                worksheet.Cell(row, 4).Value = c.Buildings?.Count ?? 0;
                worksheet.Cell(row, 5).Value = c.CreatedDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 6).Value = c.UpdatedDate?.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CampusesReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportCampusesPdf()
        {
            ForceAdminLayout();

            var result = await campusService.GetAllCampusesWithBuildingsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No campuses available to export.", "Export Campuses");
                return RedirectToAction(nameof(ManageCampuses));
            }

            using var stream = new MemoryStream();
            using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf);
                var fontBold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var fontRegular = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var blue = new iText.Kernel.Colors.DeviceRgb(40, 123, 255);

                document.Add(new iText.Layout.Element.Paragraph("KOVSIE ASSETS TRACKER - CAMPUSES REPORT")
                    .SetFont(fontBold)
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontColor(blue)
                    .SetMarginBottom(20));

                var table = new iText.Layout.Element.Table(new float[] { 2, 4, 5, 2, 2 }).UseAllAvailableWidth();
                string[] headers = { "ID", "Campus Name", "Description", "Buildings", "Created" };

                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(header)
                        .SetFont(fontBold)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                        .SetBackgroundColor(blue)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5));
                }

                foreach (var c in result.Data)
                {
                    table.AddCell(c.CampusServerId.ToString());
                    table.AddCell(c.CampusName ?? "-");
                    table.AddCell(c.CampusDescription ?? "-");
                    table.AddCell((c.Buildings?.Count ?? 0).ToString());
                    table.AddCell(c.CreatedDate?.ToString("yyyy-MM-dd") ?? "-");
                }

                document.Add(table);

                document.Add(new iText.Layout.Element.Paragraph($"\nGenerated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(fontRegular)
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY));

                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"CampusesReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        #endregion
        /*====================================================================================================================================
                                              *** EXPORT BUILDINGS ***
 =====================================================================================================================================*/
        #region Export Buildings
        [HttpGet]
        public async Task<IActionResult> ExportBuildingsExcel()
        {
            ForceAdminLayout();

            var result = await buildingService.GetAllBuildingsWithFloorsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No buildings available to export.", "Export Buildings");
                return RedirectToAction(nameof(ManageBuildings));
            }

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Buildings Report");

            worksheet.Cell(1, 1).Value = "Building ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Campus";
            worksheet.Cell(1, 5).Value = "Floors";
            worksheet.Cell(1, 6).Value = "Created";

            int row = 2;
            foreach (var b in result.Data)
            {
                worksheet.Cell(row, 1).Value = b.BuildingServerId;
                worksheet.Cell(row, 2).Value = b.BuildingName;
                worksheet.Cell(row, 3).Value = b.BuildingDescription;
                worksheet.Cell(row, 4).Value = b.CampusName ?? "-";
                worksheet.Cell(row, 5).Value = b.Floors?.Count ?? 0;
                worksheet.Cell(row, 6).Value = b.CreatedDate?.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BuildingsReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportBuildingsPdf()
        {
            ForceAdminLayout();

            var result = await buildingService.GetAllBuildingsWithFloorsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No buildings available to export.", "Export Buildings");
                return RedirectToAction(nameof(ManageBuildings));
            }

            using var stream = new MemoryStream();
            using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf);
                var fontBold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var fontRegular = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var blue = new iText.Kernel.Colors.DeviceRgb(40, 123, 255);

                document.Add(new iText.Layout.Element.Paragraph("KOVSIE ASSETS TRACKER - BUILDINGS REPORT")
                    .SetFont(fontBold)
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontColor(blue)
                    .SetMarginBottom(20));

                var table = new iText.Layout.Element.Table(new float[] { 2, 4, 5, 3, 2 }).UseAllAvailableWidth();
                string[] headers = { "ID", "Building Name", "Description", "Campus", "Floors" };

                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(header)
                        .SetFont(fontBold)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                        .SetBackgroundColor(blue)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5));
                }

                foreach (var b in result.Data)
                {
                    table.AddCell(b.BuildingServerId.ToString());
                    table.AddCell(b.BuildingName ?? "-");
                    table.AddCell(b.BuildingDescription ?? "-");
                    table.AddCell(b.CampusName ?? "-");
                    table.AddCell((b.Floors?.Count ?? 0).ToString());
                }

                document.Add(table);

                document.Add(new iText.Layout.Element.Paragraph($"\nGenerated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(fontRegular)
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY));

                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"BuildingsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        #endregion

        /*====================================================================================================================================
                                                *** EXPORT FLOORS ***
  =====================================================================================================================================*/
        #region Export Floors

        [HttpGet]
        public async Task<IActionResult> ExportFloorsExcel()
        {
            ForceAdminLayout();

            var result = await floorService.GetAllFloorsWithRoomsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No floors available to export.", "Export Floors");
                return RedirectToAction(nameof(ManageFloors));
            }

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Floors Report");

            worksheet.Cell(1, 1).Value = "Floor ID";
            worksheet.Cell(1, 2).Value = "Floor Name";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Building";
            worksheet.Cell(1, 5).Value = "Rooms";
            worksheet.Cell(1, 6).Value = "Created";

            int row = 2;
            foreach (var f in result.Data)
            {
                worksheet.Cell(row, 1).Value = f.FloorServerId;
                worksheet.Cell(row, 2).Value = f.FloorName;
                worksheet.Cell(row, 3).Value = f.FloorDescription;
                worksheet.Cell(row, 4).Value = f.BuildingName ?? "-";
                worksheet.Cell(row, 5).Value = f.Rooms?.Count ?? 0;
                worksheet.Cell(row, 6).Value = f.CreatedDate?.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"FloorsReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportFloorsPdf()
        {
            ForceAdminLayout();

            var result = await floorService.GetAllFloorsWithRoomsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No floors available to export.", "Export Floors");
                return RedirectToAction(nameof(ManageFloors));
            }

            using var stream = new MemoryStream();
            using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf);
                var fontBold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var fontRegular = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var blue = new iText.Kernel.Colors.DeviceRgb(40, 123, 255);

                document.Add(new iText.Layout.Element.Paragraph("KOVSIE ASSETS TRACKER - FLOORS REPORT")
                    .SetFont(fontBold).SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontColor(blue)
                    .SetMarginBottom(20));

                var table = new iText.Layout.Element.Table(new float[] { 2, 5, 5, 4, 2 }).UseAllAvailableWidth();
                string[] headers = { "ID", "Floor Name", "Description", "Building", "Rooms" };

                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(header)
                        .SetFont(fontBold)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                        .SetBackgroundColor(blue)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5));
                }

                foreach (var f in result.Data)
                {
                    table.AddCell(f.FloorServerId.ToString());
                    table.AddCell(f.FloorName ?? "-");
                    table.AddCell(f.FloorDescription ?? "-");
                    table.AddCell(f.BuildingName ?? "-");
                    table.AddCell((f.Rooms?.Count ?? 0).ToString());
                }

                document.Add(table);
                document.Add(new iText.Layout.Element.Paragraph($"\nGenerated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(fontRegular)
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY));

                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"FloorsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        #endregion

        /*====================================================================================================================================
                                                *** EXPORT DASHBOARD ***
  =====================================================================================================================================*/
        #region Export Dashboard

        [HttpGet]
        public async Task<IActionResult> ExportDashboardExcel()
        {
            ForceAdminLayout();

            // Gather data
            var assetsTask = assetService.GetAllAssetsAsync();
            var usersTask = userService.GetAllUsersAsync();
            var categoriesTask = categoryService.GetAllCategoriesAsync();
            var roomsTask = roomService.GetAllRoomsAsync();

            await Task.WhenAll(assetsTask, usersTask, categoriesTask, roomsTask);

            var assets = assetsTask.Result?.Data ?? new List<AssetViewModel>();
            var users = usersTask.Result?.Data ?? new List<UserViewModel>();
            var categories = categoriesTask.Result?.Data ?? new List<CategoryViewModel>();
            var rooms = roomsTask.Result?.Data ?? new List<RoomViewModel>();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Dashboard Summary");

            // === Summary section ===
            sheet.Cell("A1").Value = "KOVSIE ASSETS TRACKER - DASHBOARD SUMMARY";
            sheet.Range("A1:B1").Merge().Style
                .Font.SetBold()
                .Font.FontSize = 16;
            sheet.Cell("A3").Value = "Total Assets";
            sheet.Cell("B3").Value = assets.Count;
            sheet.Cell("A4").Value = "Total Categories";
            sheet.Cell("B4").Value = categories.Count;
            sheet.Cell("A5").Value = "Total Users";
            sheet.Cell("B5").Value = users.Count;
            sheet.Cell("A6").Value = "Total Rooms";
            sheet.Cell("B6").Value = rooms.Count;

            sheet.Range("A3:B6").Style
                .Font.SetBold()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            // === Top Assets ===
            var topAssets = assets.GroupBy(a => a.AssetName)
                .Select(g => new { AssetName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(5).ToList();

            var topAssetsSheet = workbook.Worksheets.Add("Top Assets");
            topAssetsSheet.Cell("A1").Value = "Asset Name";
            topAssetsSheet.Cell("B1").Value = "Count";
            topAssetsSheet.Range("A1:B1").Style.Font.SetBold();
            int row = 2;
            foreach (var item in topAssets)
            {
                topAssetsSheet.Cell(row, 1).Value = item.AssetName;
                topAssetsSheet.Cell(row, 2).Value = item.Count;
                row++;
            }

            // === Assets by Category ===
            var catSheet = workbook.Worksheets.Add("Assets by Category");
            catSheet.Cell("A1").Value = "Category";
            catSheet.Cell("B1").Value = "Count";
            catSheet.Range("A1:B1").Style.Font.SetBold();

            row = 2;
            var byCat = assets.GroupBy(a => a.AssetCategoryName ?? "Uncategorized")
                              .Select(g => new { g.Key, Count = g.Count() })
                              .OrderByDescending(g => g.Count);
            foreach (var c in byCat)
            {
                catSheet.Cell(row, 1).Value = c.Key;
                catSheet.Cell(row, 2).Value = c.Count;
                row++;
            }

            foreach (var ws in workbook.Worksheets)
            {
                ws.Columns().AdjustToContents();
            }


            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DashboardReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }


        [HttpGet]
        public async Task<IActionResult> ExportDashboardPdf()
        {
            ForceAdminLayout();

            // Gather data
            var assetsTask = assetService.GetAllAssetsAsync();
            var usersTask = userService.GetAllUsersAsync();
            var categoriesTask = categoryService.GetAllCategoriesAsync();
            var roomsTask = roomService.GetAllRoomsAsync();

            await Task.WhenAll(assetsTask, usersTask, categoriesTask, roomsTask);

            var assets = assetsTask.Result?.Data ?? new List<AssetViewModel>();
            var users = usersTask.Result?.Data ?? new List<UserViewModel>();
            var categories = categoriesTask.Result?.Data ?? new List<CategoryViewModel>();
            var rooms = roomsTask.Result?.Data ?? new List<RoomViewModel>();

            var totalAssets = assets.Count;
            var totalCategories = categories.Count;
            var totalUsers = users.Count;
            var totalRooms = rooms.Count;

            // Top assets summary
            var topAssets = assets.GroupBy(a => a.AssetName)
                .Select(g => new { AssetName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var blue = new DeviceRgb(40, 123, 255);
            var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var stream = new MemoryStream();
            using (var writer = new PdfWriter(stream))
            using (var pdf = new PdfDocument(writer))
            {
                var doc = new Document(pdf);
                var pageWidth = pdf.GetDefaultPageSize().GetWidth();

                // === HEADER ===
                doc.Add(new Paragraph("KOVSIE ASSETS TRACKER - DASHBOARD REPORT")
                    .SetFont(bold)
                    .SetFontSize(18)
                    .SetFontColor(blue)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                // === SUMMARY ===
                doc.Add(new Paragraph("Overall Summary")
                    .SetFont(bold).SetFontSize(14)
                    .SetFontColor(ColorConstants.BLACK));

                var summaryTable = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2 })).UseAllAvailableWidth();
                summaryTable.AddCell("Total Assets"); summaryTable.AddCell(totalAssets.ToString());
                summaryTable.AddCell("Total Categories"); summaryTable.AddCell(totalCategories.ToString());
                summaryTable.AddCell("Total Users"); summaryTable.AddCell(totalUsers.ToString());
                summaryTable.AddCell("Total Rooms"); summaryTable.AddCell(totalRooms.ToString());
                doc.Add(summaryTable);
                doc.Add(new Paragraph("\n"));

                // === TOP ASSETS ===
                doc.Add(new Paragraph("Top 5 Assets")
                    .SetFont(bold).SetFontSize(14)
                    .SetFontColor(ColorConstants.BLACK));
                var topTable = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1 })).UseAllAvailableWidth();
                topTable.AddHeaderCell(new Cell().Add(new Paragraph("Asset Name")).SetBackgroundColor(blue).SetFontColor(ColorConstants.WHITE));
                topTable.AddHeaderCell(new Cell().Add(new Paragraph("Count")).SetBackgroundColor(blue).SetFontColor(ColorConstants.WHITE));

                foreach (var t in topAssets)
                {
                    topTable.AddCell(t.AssetName ?? "-");
                    topTable.AddCell(t.Count.ToString());
                }
                doc.Add(topTable);
                doc.Add(new Paragraph("\n"));

                // === Assets by Category ===
                var byCategory = assets.GroupBy(a => a.AssetCategoryName ?? "Uncategorized")
                    .Select(g => new { g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count);

                doc.Add(new Paragraph("Assets by Category")
                    .SetFont(bold).SetFontSize(14)
                    .SetFontColor(ColorConstants.BLACK));

                var catTable = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1 })).UseAllAvailableWidth();
                catTable.AddHeaderCell(new Cell().Add(new Paragraph("Category")).SetBackgroundColor(blue).SetFontColor(ColorConstants.WHITE));
                catTable.AddHeaderCell(new Cell().Add(new Paragraph("Count")).SetBackgroundColor(blue).SetFontColor(ColorConstants.WHITE));

                foreach (var c in byCategory)
                {
                    catTable.AddCell(c.Key ?? "-");
                    catTable.AddCell(c.Count.ToString());
                }
                doc.Add(catTable);

                // === FOOTER ===
                doc.Add(new Paragraph($"\nGenerated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(regular)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.GRAY));

                doc.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"DashboardReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        #endregion
        /*====================================================================================================================================
                                                 *** EXPORT ACCESS REQUESTS ***
 ======================================================================================================================================*/
        #region Export Access Requests

        [HttpGet]
        public async Task<IActionResult> ExportAccessRequestsExcel()
        {
            var result = await accountService.GetAllAccessRequestsAsync();
            if (!result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No access requests available to export.", "Export Access Requests");
                return RedirectToAction(nameof(ManageAccessRequests));
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Access Requests Report");

            // Header
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Full Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Requested Role";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "Created At";
            worksheet.Cell(1, 7).Value = "Reviewed At";
            worksheet.Cell(1, 8).Value = "Document Path";

            int row = 2;
            foreach (var r in result.Data)
            {
                worksheet.Cell(row, 1).Value = r.Id;
                worksheet.Cell(row, 2).Value = r.FullName;
                worksheet.Cell(row, 3).Value = r.Email;
                worksheet.Cell(row, 4).Value = r.RequestedRole;
                worksheet.Cell(row, 5).Value = r.Status;
                worksheet.Cell(row, 6).Value = r.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(row, 7).Value = r.ReviewedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";
                worksheet.Cell(row, 8).Value = r.DocumentPath;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"AccessRequestsReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportAccessRequestsPdf()
        {
            var result = await accountService.GetAllAccessRequestsAsync();
            if (!result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No access requests available to export.", "Export Access Requests");
                return RedirectToAction(nameof(ManageAccessRequests));
            }

            using var stream = new MemoryStream();
            using (var writer = new PdfWriter(stream))
            using (var pdf = new PdfDocument(writer))
            {
                var document = new Document(pdf);

                var fontRegular = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var fontBold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                var blue = new DeviceRgb(40, 123, 255);
                var white = ColorConstants.WHITE;
                var gray = new DeviceRgb(240, 240, 240);

                // Header Bar
                var pageSize = pdf.GetDefaultPageSize();
                var width = pageSize.GetWidth();
                var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(pdf.AddNewPage());
                canvas.SetFillColor(blue).Rectangle(0, pageSize.GetTop() - 60, width, 60).Fill();

                var title = new Paragraph("KOVSIE ASSETS TRACKER - ACCESS REQUEST REPORT")
                    .SetFont(fontBold)
                    .SetFontSize(16)
                    .SetFontColor(white)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFixedPosition(0, pageSize.GetTop() - 45, width);
                document.Add(title);
                document.Add(new Paragraph("\n\n\n"));

                // Table
                var table = new Table(new float[] { 2, 3, 4, 3, 2, 3 }).UseAllAvailableWidth();
                string[] headers = { "ID", "Full Name", "Email", "Role", "Status", "Created" };

                foreach (var h in headers)
                {
                    table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(fontBold).SetFontColor(white))
                        .SetBackgroundColor(blue)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetPadding(5));
                }

                foreach (var r in result.Data)
                {
                    // Color status dynamically
                    var statusColor = r.Status.ToLower() switch
                    {
                        "approved" => new DeviceRgb(76, 175, 80),
                        "declined" => new DeviceRgb(244, 67, 54),
                        _ => new DeviceRgb(255, 193, 7)
                    };

                    table.AddCell(new Cell().Add(new Paragraph(r.Id.ToString())).SetBackgroundColor(gray));
                    table.AddCell(new Cell().Add(new Paragraph(r.FullName ?? "-")));
                    table.AddCell(new Cell().Add(new Paragraph(r.Email ?? "-")));
                    table.AddCell(new Cell().Add(new Paragraph(r.RequestedRole ?? "-")));
                    table.AddCell(new Cell().Add(new Paragraph(r.Status ?? "Pending"))
                        .SetFontColor(white)
                        .SetBackgroundColor(statusColor)
                        .SetTextAlignment(TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(r.CreatedAt.ToString("yyyy-MM-dd"))));
                }

                document.Add(table);

                document.Add(new Paragraph($"\nReport generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(fontRegular)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.GRAY));

                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"AccessRequestsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        #endregion

        /*====================================================================================================================================
                                                       *** HELPERS ***
         ======================================================================================================================================*/
        #region Helpers
        private void ForceAdminLayout()
        {
            var redirect = RequireRole("Administrator");
            if (redirect != null) RedirectToAction(redirect.ToString());

            ForceLayout();

        }
        #endregion

    }
}
