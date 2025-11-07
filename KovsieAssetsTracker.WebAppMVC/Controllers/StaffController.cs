using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using iText.Kernel.Pdf;
using iText.StyledXmlParser.Jsoup.Nodes;
using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Models.Asset;
using KovsieAssetsTracker.WebAppMVC.Models.Category;
using KovsieAssetsTracker.WebAppMVC.Models.Floor;
using KovsieAssetsTracker.WebAppMVC.Models.Room;
using KovsieAssetsTracker.WebAppMVC.Models.Staff;
using KovsieAssetsTracker.WebAppMVC.Models.User;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using KovsieAssetsTracker.WebAppMVC.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KovsieAssetsTracker.WebAppMVC.Controllers
{
    public class StaffController : BaseController
    {
        private readonly IAccountService accountService;
        private readonly IUserService userService;
        private readonly IAssetService assetService;
        private readonly ICategoryService categoryService;
        private readonly IRoomService roomService;
        private readonly IFloorService floorService;

        public StaffController(IAccountService _accountService, IUserService _userService, IAssetService _assetService, ICategoryService _categoryService, IRoomService _roomService, IFloorService _floorService, SessionManager _sessionManager) : base(_sessionManager)
        {
            accountService = _accountService;
            userService = _userService;
            assetService = _assetService;
            categoryService = _categoryService;
            roomService = _roomService;
            floorService = _floorService;
        }
        /*====================================================================================================================================
                                                                *** DASHBOARD ***
        ======================================================================================================================================*/
        #region Dashboard
        public async Task<IActionResult> StaffDashboard()
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            // Load from services
            var assetsResponse = await assetService.GetAllAssetsAsync();
            var categoriesResponse = await categoryService.GetAllCategoriesWithAssetsAsync();
            var roomsResponse = await roomService.GetAllRoomsAsync();

            var totalAssets = assetsResponse.Data?.Count ?? 0;
            var totalCategories = categoriesResponse.Data?.Count ?? 0;
            var totalRooms = roomsResponse.Data?.Count ?? 0;

            // Ensure we always have valid data lists
            var categoryLabels = categoriesResponse.Data?
                .Select(c => c.AssetCategoryName ?? "Uncategorized").ToList() ?? new();
            var categoryCounts = categoriesResponse.Data?
                .Select(c => c.Assets?.Count ?? 0).ToList() ?? new();

            var roomLabels = roomsResponse.Data?
                .Select(r => r.RoomName ?? "Unnamed Room").ToList() ?? new();
            var roomCounts = roomsResponse.Data?
                .Select(r => r.Assets?.Count ?? 0).ToList() ?? new();

            // Clean mismatched lengths
            int minCategoryCount = Math.Min(categoryLabels.Count, categoryCounts.Count);
            int minRoomCount = Math.Min(roomLabels.Count, roomCounts.Count);

            categoryLabels = categoryLabels.Take(minCategoryCount).ToList();
            categoryCounts = categoryCounts.Take(minCategoryCount).ToList();
            roomLabels = roomLabels.Take(minRoomCount).ToList();
            roomCounts = roomCounts.Take(minRoomCount).ToList();

            // Recent items
            var recentAssets = assetsResponse.Data?
                .OrderByDescending(a => a.UpdatedDate ?? a.CreatedDate)
                .Take(5).ToList() ?? new();
            var recentRooms = roomsResponse.Data?
                .OrderByDescending(r => r.UpdatedDate ?? r.CreatedDate)
                .Take(5).ToList() ?? new();

            // Build ViewModel
            var model = new StaffDashboardViewModel
            {
                TotalAssets = totalAssets,
                TotalCategories = totalCategories,
                TotalRooms = totalRooms,
                CategoryLabels = categoryLabels,
                CategoryCounts = categoryCounts,
                RoomLabels = roomLabels,
                RoomCounts = roomCounts,
                RecentAssets = recentAssets,
                RecentRooms = recentRooms
            };

            TempData.SetAlert("info", $"Welcome back, {sessionManager.GetUserDisplayName()}!", "Staff Dashboard");
            return View(model);
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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            // Get asset, categories, and rooms in parallel for performance
            var assetTask = assetService.GetAssetByIdAsync(id);
            var categoriesTask = categoryService.GetAllCategoriesAsync();
            var roomsTask = roomService.GetAllRoomsAsync();

            await Task.WhenAll(assetTask, categoriesTask, roomsTask);

            var result = assetTask.Result;
            var categoriesResult = categoriesTask.Result;
            var roomsResult = roomsTask.Result;

            if (result == null || !result.Success || result.Data == null)
            {
                TempData.SetAlert("error", "Asset not found or failed to load data.", "Manage Assets");
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
                Categories = categoriesResult?.Data ?? new List<CategoryViewModel>(),
                Rooms = roomsResult?.Data ?? new List<RoomViewModel>()
            };

            TempData["AssetServerId"] = id;
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssetsDetailsAndUpdate(UpdateAssetViewModel model)
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            // Retain asset ID if TempData still holds it
            if (TempData["AssetServerId"] != null && model.AssetServerId == 0)
                model.AssetServerId = Convert.ToInt32(TempData["AssetServerId"]);

            // Always load dependent data first (categories and rooms)
            var categoriesResult = await categoryService.GetAllCategoriesAsync();
            var roomsResult = await roomService.GetAllRoomsAsync();

            // Load asset details in case of redisplay or failed update
            var assetResult = await assetService.GetAssetByIdAsync(model.AssetServerId);

            // If the form is invalid, rebuild view with all data preserved
            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct highlighted errors before submitting.", "Manage Assets");

                var detailsVm = new DetailsAndUpdateAssetViewModel
                {
                    AssetDetails = assetResult?.Data ?? new AssetViewModel
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
                    },
                    UpdateAsset = model,
                    Categories = categoriesResult?.Data ?? new List<CategoryViewModel>(),
                    Rooms = roomsResult?.Data ?? new List<RoomViewModel>()
                };

                return View(detailsVm);
            }

            // Perform the update
            var updateResult = await assetService.UpdateAssetAsync(model);

            // On success
            if (updateResult.Success)
            {
                TempData.SetAlert("success", "Asset updated successfully!", "Manage Assets");
                return RedirectToAction(nameof(AssetsDetailsAndUpdate), new { id = model.AssetServerId });
            }

            // On failure, reload everything and redisplay
            TempData.SetAlert("error", updateResult.Error ?? "Failed to update asset.", "Manage Assets");

            var vm = new DetailsAndUpdateAssetViewModel
            {
                AssetDetails = assetResult?.Data ?? new AssetViewModel
                {
                    AssetServerId = model.AssetServerId,
                    AssetName = model.AssetName,
                    AssetDescription = model.AssetDescription,
                    Status = model.Status,
                    AssetCategoryServerId = model.AssetCategoryServerId,
                    RoomServerId = model.RoomServerId,
                    IsAndroidBackedUp = model.IsAndroidBackedUp,
                    IsServerBackedUp = model.IsServerBackedUp
                },
                UpdateAsset = model,
                Categories = categoriesResult?.Data ?? new List<CategoryViewModel>(),
                Rooms = roomsResult?.Data ?? new List<RoomViewModel>()
            };

            return View(vm);
        }

        #endregion


        #region Delete Asset - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsset(int assetId)
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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

            // On failure: repopulate list + keep form values
            var allCategories = await categoryService.GetAllCategoriesWithAssetsAsync();
            model.Categories = allCategories.Data ?? new List<CategoryViewModel>();

            TempData.SetAlert("error", result.Error ?? "Failed to create category.", "Manage Categories");
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> CategoriesDetailsAndUpdate(int id)
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            var result = await categoryService.GetCategoryByIdAsync(id);
            if (result == null || !result.Success)
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
                    AssetCategoryLocalId = result.Data.AssetCategoryLocalId,
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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            if (TempData["AssetCategoryServerId"] != null && model.AssetCategoryServerId == 0)
                model.AssetCategoryServerId = Convert.ToInt32(TempData["AssetCategoryServerId"]);

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct errors.", "Manage Categories");
                return RedirectToAction(nameof(CategoriesDetailsAndUpdate), new { id = model.AssetCategoryServerId });
            }

            var updateResult = await categoryService.UpdateCategoryAsync(model);
            if (updateResult.Success)
            {
                TempData.SetAlert("success", "Category updated successfully!", "Manage Categories");
                return RedirectToAction(nameof(CategoriesDetailsAndUpdate), new { id = model.AssetCategoryServerId });
            }

            TempData.SetAlert("error", updateResult.Error ?? "Failed to update category.", "Manage Categories");
            return RedirectToAction(nameof(CategoriesDetailsAndUpdate), new { id = model.AssetCategoryServerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            var result = await categoryService.DeleteCategoryAsync(categoryId);
            if (result != null && result.Success)
                TempData.SetAlert("success", "Category deleted successfully.", "Manage Categories");
            else
                TempData.SetAlert("error", "Failed to delete category. Please try again.", "Manage Categories");

            return RedirectToAction(nameof(ManageCategories));
        }
        #endregion

        /*====================================================================================================================================
                                                            *** MANAGE ROOMS ***
        ======================================================================================================================================*/
        #region Manage Rooms
        [HttpGet]
        public async Task<IActionResult> ManageRooms()
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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

        [HttpGet]
        public async Task<IActionResult> RoomsDetailsAndUpdate(int id)
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            var result = await roomService.GetRoomByIdAsync(id);
            var floorsResult = await floorService.GetAllFloorsAsync(); 

            if (result == null || !result.Success)
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
                    RoomLocalId = result.Data.RoomLocalId,
                    RoomName = result.Data.RoomName,
                    RoomDescription = result.Data.RoomDescription,
                    IsAndroidBackedUp = result.Data.IsAndroidBackedUp,
                    IsServerBackedUp = result.Data.IsServerBackedUp,
                    FloorServerId = result.Data.FloorServerId
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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            if (TempData["RoomServerId"] != null && model.RoomServerId == 0)
                model.RoomServerId = Convert.ToInt32(TempData["RoomServerId"]);

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct errors.", "Manage Rooms");
                return RedirectToAction(nameof(RoomsDetailsAndUpdate), new { id = model.RoomServerId });
            }

            var updateResult = await roomService.UpdateRoomAsync(model);
            if (updateResult.Success)
            {
                TempData.SetAlert("success", "Room updated successfully!", "Manage Rooms");
                return RedirectToAction(nameof(RoomsDetailsAndUpdate), new { id = model.RoomServerId });
            }

            TempData.SetAlert("error", updateResult.Error ?? "Failed to update room.", "Manage Rooms");
            return RedirectToAction(nameof(RoomsDetailsAndUpdate), new { id = model.RoomServerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoom(int roomId)
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            var result = await roomService.DeleteRoomAsync(roomId);
            if (result != null && result.Success)
                TempData.SetAlert("success", "Room deleted successfully.", "Manage Rooms");
            else
                TempData.SetAlert("error", "Failed to delete room.", "Manage Rooms");

            return RedirectToAction(nameof(ManageRooms));
        }
        #endregion
        /*====================================================================================================================================
                                                        *** SEARCH MANAGER ***
=====================================================================================================================================*/
        #region Search Manager

        [HttpGet]
        public async Task<IActionResult> SearchManager(string? query, string? typeFilter)
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            var vm = await BuildSearchResults(query, typeFilter);
            vm.TypeFilter = typeFilter ?? "All";
            return View(vm);
        }

        // 🔹 Reusable search builder with optional filtering
        private async Task<SearchManagerViewModel> BuildSearchResults(string? query, string? typeFilter = "All")
        {
            var model = new SearchManagerViewModel { Query = query ?? "", TypeFilter = typeFilter ?? "All" };

            if (string.IsNullOrWhiteSpace(query))
                return model;

            query = query.Trim().ToLowerInvariant();
            typeFilter = typeFilter?.ToLowerInvariant() ?? "all";

            var assets = await assetService.GetAllAssetsAsync();
            var rooms = await roomService.GetAllRoomsAsync();
            var categories = await categoryService.GetAllCategoriesAsync();

            var results = new List<SearchResultItemViewModel>();

            // --- Assets ---
            if (typeFilter == "asset" || typeFilter == "all")
            {
                foreach (var a in assets.Data ?? new())
                {
                    if ((a.AssetName?.ToLower().Contains(query) ?? false) ||
                        (a.AssetDescription?.ToLower().Contains(query) ?? false) ||
                        (a.AssetCategoryName?.ToLower().Contains(query) ?? false))
                    {
                        results.Add(new SearchResultItemViewModel
                        {
                            Type = "Asset",
                            Id = a.AssetServerId,
                            Title = a.AssetName ?? "Unnamed Asset",
                            Description = a.AssetDescription ?? "No description",
                            Extra = $"Category: {a.AssetCategoryName ?? "-"} | Room: {a.RoomName ?? "-"}",
                            CreatedDate = a.CreatedDate
                        });
                    }
                }
            }

            // --- Rooms ---
            if (typeFilter == "room" || typeFilter == "all")
            {
                foreach (var r in rooms.Data ?? new())
                {
                    if ((r.RoomName?.ToLower().Contains(query) ?? false) ||
                        (r.RoomDescription?.ToLower().Contains(query) ?? false))
                    {
                        results.Add(new SearchResultItemViewModel
                        {
                            Type = "Room",
                            Id = r.RoomServerId,
                            Title = r.RoomName ?? "Unnamed Room",
                            Description = r.RoomDescription ?? "No description",
                            Extra = $"Floor: {r.FloorName ?? "-"} | Assets: {r.Assets?.Count ?? 0}",
                            CreatedDate = r.CreatedDate
                        });
                    }
                }
            }

            // --- Categories ---
            if (typeFilter == "category" || typeFilter == "all")
            {
                foreach (var c in categories.Data ?? new())
                {
                    if ((c.AssetCategoryName?.ToLower().Contains(query) ?? false) ||
                        (c.AssetCategoryDescription?.ToLower().Contains(query) ?? false))
                    {
                        results.Add(new SearchResultItemViewModel
                        {
                            Type = "Category",
                            Id = c.AssetCategoryServerId,
                            Title = c.AssetCategoryName ?? "Unnamed Category",
                            Description = c.AssetCategoryDescription ?? "No description",
                            Extra = $"Assets: {c.Assets?.Count ?? 0}",
                            CreatedDate = c.CreatedDate
                        });
                    }
                }
            }

            model.Results = results.OrderByDescending(r => r.CreatedDate).ToList();
            return model;
        }

        #endregion




        /*====================================================================================================================================
         *** PROFILE ***
        ======================================================================================================================================*/
        #region PROFILE

        [HttpGet]
        public async Task<IActionResult> ManageProfile()
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null) return layoutRedirect;

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
                return RedirectToAction(nameof(StaffDashboard));
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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null) return layoutRedirect;

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
                                                        *** CHANGE PASSWORD ***
        ======================================================================================================================================*/
        #region CHANGE PASSWORD

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            var vm = new ChangePasswordViewModel
            {
                UserId = sessionManager.GetUserId()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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

        #endregion



        /*====================================================================================================================================
                                                                *** LOG OUT ***
        ======================================================================================================================================*/
        #region LOG OUT
        public IActionResult LogOut()
        {
            ForceStaffLayout();
            sessionManager.Clear();

            TempData.SetAlert("info", "You have been signed out.", "Session Ended");
            return RedirectToAction("Login", "Home");
        }
        #endregion



        /*====================================================================================================================================
                                                                  *** HELPERS ***
         ======================================================================================================================================*/
        #region Helpers
        private IActionResult? ForceStaffLayout()
        {
            var redirect = RequireRole("StaffMember");
            if (redirect != null)
                return RedirectToAction(redirect.ToString());

            ForceLayout();
            return null;
        }

        #endregion

        /*====================================================================================================================================
                                         *** EXPORT ASSETS  ***
         =====================================================================================================================================*/
        #region Export Assets
        [HttpGet]
        public async Task<IActionResult> ExportAssetsExcel()
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            var result = await roomService.GetAllRoomsAsync();
            if (result == null || !result.Success || result.Data == null || !result.Data.Any())
            {
                TempData.SetAlert("error", "No rooms available to export.", "Export Rooms");
                return RedirectToAction(nameof(ManageRooms));
            }

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Rooms Report");

            // Header Row
            worksheet.Cell(1, 1).Value = "Room ID";
            worksheet.Cell(1, 2).Value = "Room Name";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Floor";
            worksheet.Cell(1, 5).Value = "Android Backup";
            worksheet.Cell(1, 6).Value = "Server Backup";
            worksheet.Cell(1, 7).Value = "Created Date";

            int row = 2;
            foreach (var r in result.Data)
            {
                worksheet.Cell(row, 1).Value = r.RoomServerId;
                worksheet.Cell(row, 2).Value = r.RoomName;
                worksheet.Cell(row, 3).Value = r.RoomDescription;
                worksheet.Cell(row, 4).Value = string.IsNullOrEmpty(r.FloorName) ? "Unassigned" : r.FloorName;
                worksheet.Cell(row, 5).Value = r.IsAndroidBackedUp ? "Yes" : "No";
                worksheet.Cell(row, 6).Value = r.IsServerBackedUp ? "Yes" : "No";
                worksheet.Cell(row, 7).Value = r.CreatedDate?.ToString("yyyy-MM-dd");
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
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

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
                var white = iText.Kernel.Colors.ColorConstants.WHITE;

                // Header background
                var pageSize = pdf.GetDefaultPageSize();
                var width = pageSize.GetWidth();
                var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(pdf.AddNewPage());
                canvas.SetFillColor(blue)
                      .Rectangle(0, pageSize.GetTop() - 60, width, 60)
                      .Fill();

                // Title
                var title = new iText.Layout.Element.Paragraph("KOVSIE ASSETS TRACKER - ROOMS REPORT")
                    .SetFont(fontBold)
                    .SetFontSize(16)
                    .SetFontColor(white)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFixedPosition(0, pageSize.GetTop() - 45, width);
                document.Add(title);
                document.Add(new iText.Layout.Element.Paragraph("\n\n\n"));

                // Table setup
                var table = new iText.Layout.Element.Table(new float[] { 2, 3, 4, 2, 2, 2, 2 }).UseAllAvailableWidth();
                string[] headers = { "ID", "Name", "Description", "Floor", "Android Backup", "Server Backup", "Created" };

                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(header)
                            .SetFont(fontBold)
                            .SetFontColor(white))
                        .SetBackgroundColor(blue)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5));
                }

                // Table Data
                foreach (var r in result.Data)
                {
                    table.AddCell(r.RoomServerId.ToString());
                    table.AddCell(r.RoomName ?? "-");
                    table.AddCell(r.RoomDescription ?? "-");
                    table.AddCell(string.IsNullOrEmpty(r.FloorName) ? "Unassigned" : r.FloorName);
                    table.AddCell(r.IsAndroidBackedUp ? "Yes" : "No");
                    table.AddCell(r.IsServerBackedUp ? "Yes" : "No");
                    table.AddCell(r.CreatedDate?.ToString("yyyy-MM-dd") ?? "-");
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

            return File(stream.ToArray(), "application/pdf", $"RoomsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        #endregion

        /*====================================================================================================================================
                                   *** EXPORT SEARCH RESULTS  ***
 =====================================================================================================================================*/
        #region Export Search Results

        [HttpGet]
        public async Task<IActionResult> ExportSearchResultExcel(string? query)
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            var vm = await BuildSearchResults(query);
            if (vm.Results == null || !vm.Results.Any())
            {
                TempData.SetAlert("error", "No search results to export.", "Search Manager");
                return RedirectToAction(nameof(SearchManager));
            }

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Search Results");
            ws.Cell(1, 1).Value = "Type";
            ws.Cell(1, 2).Value = "Title";
            ws.Cell(1, 3).Value = "Description";
            ws.Cell(1, 4).Value = "Extra";
            ws.Cell(1, 5).Value = "Created Date";

            int row = 2;
            foreach (var r in vm.Results)
            {
                ws.Cell(row, 1).Value = r.Type;
                ws.Cell(row, 2).Value = r.Title;
                ws.Cell(row, 3).Value = r.Description;
                ws.Cell(row, 4).Value = r.Extra;
                ws.Cell(row, 5).Value = r.CreatedDate?.ToString("yyyy-MM-dd");
                row++;
            }

            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"SearchResults_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }


        [HttpGet]
        public async Task<IActionResult> ExportSearchResultPdf(string? query)
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            var vm = await BuildSearchResults(query);
            if (vm.Results == null || !vm.Results.Any())
            {
                TempData.SetAlert("error", "No search results to export.", "Search Manager");
                return RedirectToAction(nameof(SearchManager));
            }

            using var stream = new MemoryStream();
            using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
            {
                var doc = new iText.Layout.Document(pdf);
                var blue = new iText.Kernel.Colors.DeviceRgb(40, 123, 255);
                var white = iText.Kernel.Colors.ColorConstants.WHITE;
                var bold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                doc.Add(new iText.Layout.Element.Paragraph($"Search Results for \"{query}\"")
                    .SetFont(bold).SetFontSize(14)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontColor(blue).SetMarginBottom(15));

                var table = new iText.Layout.Element.Table(new float[] { 2, 3, 4, 3, 2 }).UseAllAvailableWidth();
                string[] headers = { "Type", "Title", "Description", "Extra", "Created" };

                foreach (var h in headers)
                    table.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(h))
                        .SetBackgroundColor(blue)
                        .SetFontColor(white)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5));

                foreach (var r in vm.Results)
                {
                    table.AddCell(r.Type ?? "-");
                    table.AddCell(r.Title ?? "-");
                    table.AddCell(r.Description ?? "-");
                    table.AddCell(r.Extra ?? "-");
                    table.AddCell(r.CreatedDate?.ToString("yyyy-MM-dd") ?? "-");
                }

                doc.Add(table);
                doc.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"SearchResults_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        #endregion



        /*====================================================================================================================================
                                 *** EXPORT DASHBOARD  ***
        =====================================================================================================================================*/

        #region Export Dashboard
        [HttpGet]
     
        public async Task<IActionResult> ExportDashboardExcel()
        {
            var layoutRedirect = ForceStaffLayout();
            if (layoutRedirect != null)
                return layoutRedirect;

            // Get dashboard data
            var assetsResponse = await assetService.GetAllAssetsAsync();
            var categoriesResponse = await categoryService.GetAllCategoriesWithAssetsAsync();
            var roomsResponse = await roomService.GetAllRoomsAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            // ===== SUMMARY SHEET =====
            var summary = workbook.Worksheets.Add("Summary");
            summary.Cell("A1").Value = "Dashboard Summary";
            summary.Range("A1:D1").Merge().Style
                .Font.SetBold().Font.FontSize = 16;
            summary.Cell("A3").Value = "Total Assets";
            summary.Cell("B3").Value = assetsResponse.Data?.Count ?? 0;
            summary.Cell("A4").Value = "Total Categories";
            summary.Cell("B4").Value = categoriesResponse.Data?.Count ?? 0;
            summary.Cell("A5").Value = "Total Rooms";
            summary.Cell("B5").Value = roomsResponse.Data?.Count ?? 0;

            // ===== CATEGORY CHART DATA =====
            var wsCategory = workbook.Worksheets.Add("Assets by Category");
            wsCategory.Cell(1, 1).Value = "Category";
            wsCategory.Cell(1, 2).Value = "Asset Count";

            int row = 2;
            foreach (var c in categoriesResponse.Data ?? new())
            {
                wsCategory.Cell(row, 1).Value = c.AssetCategoryName ?? "Uncategorized";
                wsCategory.Cell(row, 2).Value = c.Assets?.Count ?? 0;
                row++;
            }

            wsCategory.Columns().AdjustToContents();

            // ===== ROOM CHART DATA =====
            var wsRoom = workbook.Worksheets.Add("Assets by Room");
            wsRoom.Cell(1, 1).Value = "Room";
            wsRoom.Cell(1, 2).Value = "Asset Count";
            row = 2;
            foreach (var r in roomsResponse.Data ?? new())
            {
                wsRoom.Cell(row, 1).Value = r.RoomName ?? "Unnamed Room";
                wsRoom.Cell(row, 2).Value = r.Assets?.Count ?? 0;
                row++;
            }

            wsRoom.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"StaffDashboard_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        [HttpGet]
        public async Task<IActionResult> ExportDashboardPdf()
        {
            ForceStaffLayout();

            var assetsResponse = await assetService.GetAllAssetsAsync();
            var categoriesResponse = await categoryService.GetAllCategoriesWithAssetsAsync();
            var roomsResponse = await roomService.GetAllRoomsAsync();

            using var stream = new MemoryStream();
            using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf);
                var pageSize = pdf.GetDefaultPageSize();
                var width = pageSize.GetWidth();

                // ==== FONTS ====
                var fontRegular = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var fontBold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                // ==== COLORS ====
                var blue = new iText.Kernel.Colors.DeviceRgb(40, 123, 255);   // Kovsie Blue
                var peach = new iText.Kernel.Colors.DeviceRgb(255, 182, 123); // Kovsie Peach
                var white = iText.Kernel.Colors.ColorConstants.WHITE;

                // ==== HEADER BAR (Gradient Effect) ====
                var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(pdf.AddNewPage());
                var gradient = new iText.Kernel.Pdf.Extgstate.PdfExtGState();
                gradient.SetFillOpacity(0.9f);
                canvas.SaveState()
                      .SetExtGState(gradient)
                      .SetFillColor(blue)
                      .Rectangle(0, pageSize.GetTop() - 60, width, 60)
                      .Fill()
                      .RestoreState();

                // ==== ADD LOGO (Optional) ====
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "icon_logo_transparent.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(logoPath))
                        .ScaleToFit(60, 60)
                        .SetFixedPosition(40, pageSize.GetTop() - 55);
                    document.Add(logo);
                }

                // ==== TITLE ON HEADER ====
                var title = new iText.Layout.Element.Paragraph("KOVSIE ASSETS TRACKER")
                    .SetFont(fontBold)
                    .SetFontSize(18)
                    .SetFontColor(white)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFixedPosition(0, pageSize.GetTop() - 45, width);
                document.Add(title);

                // === ADD SPACING BELOW HEADER ===
                document.Add(new iText.Layout.Element.Paragraph("\n\n\n"));

                // ==== SUMMARY SECTION ====
                document.Add(new iText.Layout.Element.Paragraph("Dashboard Summary")
                    .SetFont(fontBold)
                    .SetFontSize(14)
                    .SetFontColor(blue)
                    .SetMarginBottom(8));

                document.Add(new iText.Layout.Element.Paragraph(
                    $"Total Assets: {assetsResponse.Data?.Count ?? 0}\n" +
                    $"Total Categories: {categoriesResponse.Data?.Count ?? 0}\n" +
                    $"Total Rooms: {roomsResponse.Data?.Count ?? 0}\n"
                ).SetFont(fontRegular).SetFontSize(12)
                 .SetMarginBottom(20));

                // ==== CATEGORY TABLE ====
                document.Add(new iText.Layout.Element.Paragraph("Assets by Category")
                    .SetFont(fontBold)
                    .SetFontSize(13)
                    .SetFontColor(blue)
                    .SetMarginBottom(5));

                var categoryTable = new iText.Layout.Element.Table(new float[] { 3, 2 }).UseAllAvailableWidth();
                categoryTable.AddHeaderCell(new iText.Layout.Element.Cell()
                    .Add(new iText.Layout.Element.Paragraph("Category").SetFont(fontBold).SetFontColor(white))
                    .SetBackgroundColor(blue).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                categoryTable.AddHeaderCell(new iText.Layout.Element.Cell()
                    .Add(new iText.Layout.Element.Paragraph("Asset Count").SetFont(fontBold).SetFontColor(white))
                    .SetBackgroundColor(blue).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                foreach (var c in categoriesResponse.Data ?? new())
                {
                    categoryTable.AddCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(c.AssetCategoryName ?? "Uncategorized").SetFont(fontRegular)));
                    categoryTable.AddCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph((c.Assets?.Count ?? 0).ToString()).SetFont(fontRegular))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                }

                document.Add(categoryTable);
                document.Add(new iText.Layout.Element.Paragraph("\n"));

                // ==== ROOM TABLE ====
                document.Add(new iText.Layout.Element.Paragraph("Assets by Room")
                    .SetFont(fontBold)
                    .SetFontSize(13)
                    .SetFontColor(blue)
                    .SetMarginBottom(5));

                var roomTable = new iText.Layout.Element.Table(new float[] { 3, 2 }).UseAllAvailableWidth();
                roomTable.AddHeaderCell(new iText.Layout.Element.Cell()
                    .Add(new iText.Layout.Element.Paragraph("Room").SetFont(fontBold).SetFontColor(white))
                    .SetBackgroundColor(blue).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                roomTable.AddHeaderCell(new iText.Layout.Element.Cell()
                    .Add(new iText.Layout.Element.Paragraph("Asset Count").SetFont(fontBold).SetFontColor(white))
                    .SetBackgroundColor(blue).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                foreach (var r in roomsResponse.Data ?? new())
                {
                    roomTable.AddCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(r.RoomName ?? "Unnamed Room").SetFont(fontRegular)));
                    roomTable.AddCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph((r.Assets?.Count ?? 0).ToString()).SetFont(fontRegular))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                }

                document.Add(roomTable);

                // ==== FOOTER ====
                document.Add(new iText.Layout.Element.Paragraph(
                    $"\nReport Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(fontRegular)
                    .SetFontSize(10)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginTop(30));

                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"StaffDashboard_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        #endregion
    }
}
