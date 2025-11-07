using KovsieAssetsTracker.Application.DTOs.SharedDTOs;
using KovsieAssetsTracker.Application.DTOs.UserDTO;
using KovsieAssetsTracker.Application.Helpers;
using KovsieAssetsTracker.Application.Services;
using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;
using KovsieAssetsTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KovsieAssetsTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment env;
        private readonly IRepositoryWrapper repo;
        private readonly IConfiguration config;
        private readonly IEmailSender emailSender;

        private static readonly string folderName = "users";
        private static readonly string defaultImage = "default-user.png";

        public UsersController(UserManager<ApplicationUser> _userManager, IWebHostEnvironment _env, IRepositoryWrapper _repo, IConfiguration _config, IEmailSender _emailSender)
        {
            userManager = _userManager;
            env = _env;
            repo = _repo;
            config = _config;
            emailSender = _emailSender;
        }

        private string GetBaseUrl(string folder)
            => $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/{folder}/";

        private async Task<string> SaveImageAsync(IFormFile? file)
        {
            if (file is { Length: > 0 })
            {
                var folder = Path.Combine(env.WebRootPath, "images", folderName);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var path = Path.Combine(folder, fileName);

                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                return fileName;
            }
            return defaultImage;
        }

        /// <summary>Retrieve all users (Admin only).</summary>
        [HttpGet]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userManager.Users.ToListAsync();
            var baseUrl = GetBaseUrl(folderName);

            var response = new List<UserResponseDTO>();
            foreach (var u in users)
            {
                var role = (await userManager.GetRolesAsync(u)).FirstOrDefault() ?? ConstantsUtils.STAFF_ROLE;

                response.Add(new UserResponseDTO
                {
                    UserId = u.Id,
                    UsernameDisplay = u.UsernameDisplay,
                    UserEmail = u.Email,
                    StudentStaffNumber = u.StudentStaffNumber,
                    ProfileImageFileName = baseUrl + (u.ProfileImageFileName ?? defaultImage),
                    ContactNumber = u.ContactNumber,
                    Description = u.Description,
                    UserRole = role,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate
                });
            }

            return Ok(ApiResponseDTO<List<UserResponseDTO>>.SuccessResponse(response, "Users retrieved successfully."));
        }

        /// <summary>Retrieve a specific user by ID (Admin only).</summary>
        [HttpGet("get_user/{id}")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE},{ConstantsUtils.AGENT_ROLE}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? ConstantsUtils.STAFF_ROLE;
            var response = new UserResponseDTO
            {
                UserId = user.Id,
                UsernameDisplay = user.UsernameDisplay,
                UserEmail = user.Email,
                StudentStaffNumber = user.StudentStaffNumber,
                ProfileImageFileName = GetBaseUrl(folderName) + (user.ProfileImageFileName ?? defaultImage),
                ContactNumber = user.ContactNumber,
                Description = user.Description,
                UserRole = role,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate
            };

            return Ok(ApiResponseDTO<UserResponseDTO>.SuccessResponse(response, "User retrieved successfully."));
        }

        /// <summary>Update user details (Admin can update anyone, Staff/Agent can update their own profile).</summary>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE},{ConstantsUtils.AGENT_ROLE}")]
        public async Task<IActionResult> UpdateUser(string id, [FromForm] UpdateUserRequestDTO dto)
        {
            dto.UserId = id;
            var user = await repo.User.GetUserByIdAsync(dto.UserId);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            // Apply updates
            user.UsernameDisplay = dto.UsernameDisplay ?? user.UsernameDisplay;
            user.Email = dto.UserEmail ?? user.Email;
            user.UserName = dto.UserEmail ?? user.UserName;
            user.StudentStaffNumber = dto.StudentStaffNumber ?? user.StudentStaffNumber;
            user.ContactNumber = dto.ContactNumber ?? user.ContactNumber;
            user.Description = dto.Description ?? user.Description;
            user.UpdatedDate = DateTime.UtcNow;

            // Save new profile image if provided
            if (dto.ImageFile is { Length: > 0 })
                user.ProfileImageFileName = await SaveImageAsync(dto.ImageFile);

            await repo.User.UpdateUserAsync(user);

            // Update roles (only Admins can change role)
            if (User.IsInRole(ConstantsUtils.ADMIN_ROLE) && !string.IsNullOrEmpty(dto.UserRole))
            {
                var existingRoles = await userManager.GetRolesAsync(user);
                if (existingRoles.Any())
                    await userManager.RemoveFromRolesAsync(user, existingRoles);

                await userManager.AddToRoleAsync(user, dto.UserRole);
            }

            var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? ConstantsUtils.STAFF_ROLE;
            var response = new UserResponseDTO
            {
                UserId = user.Id,
                UsernameDisplay = user.UsernameDisplay,
                UserEmail = user.Email,
                StudentStaffNumber = user.StudentStaffNumber,
                ProfileImageFileName = GetBaseUrl(folderName) + (user.ProfileImageFileName ?? defaultImage),
                ContactNumber = user.ContactNumber,
                Description = user.Description,
                UserRole = role,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate
            };

            return Ok(ApiResponseDTO<UserResponseDTO>.SuccessResponse(response, "User updated successfully."));
        }

        /// <summary>Change profile image (self or Admin).</summary>
        [HttpPost("{id}/profile-image")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE},{ConstantsUtils.AGENT_ROLE}")]
        public async Task<IActionResult> ChangeProfileImage(string id, IFormFile file)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            var currentUserId = userManager.GetUserId(User);
            if (!User.IsInRole(ConstantsUtils.ADMIN_ROLE) && currentUserId != id)
                return Forbid();

            if (file is { Length: > 0 })
                user.ProfileImageFileName = await SaveImageAsync(file);

            await userManager.UpdateAsync(user);

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Profile picture updated successfully."));
        }

        /// <summary>Change user password (self only).</summary>
        [HttpPost("change-password")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.STAFF_ROLE},{ConstantsUtils.AGENT_ROLE}")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO dto)
        {
            var user = await userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            var currentUserId = userManager.GetUserId(User);
            if (currentUserId != dto.UserId)
                return Forbid();

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Failed to update password.", result.Errors.FirstOrDefault()?.Description));

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Password changed successfully."));
        }

        /// <summary>Reset another user's password (Admin only).</summary>
        [HttpPost("admin-reset-password")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> AdminResetUserPassword([FromForm] string email, [FromForm] string temporaryPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(temporaryPassword))
                return BadRequest(ApiResponseDTO<string>.FailResponse("Email and temporary password are required."));

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            try
            {
                // 1️⃣  Generate reset token
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

                // 2️⃣  Reset password using temporary value
                var resetResult = await userManager.ResetPasswordAsync(user, resetToken, temporaryPassword);
                if (!resetResult.Succeeded)
                {
                    var errors = string.Join("; ", resetResult.Errors.Select(e => e.Description));
                    return BadRequest(ApiResponseDTO<string>.FailResponse($"Failed to reset password: {errors}"));
                }

                // 3️⃣  Send Kovsie-branded notification email
                string logoUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/icon_logo_transparent.png";
                string loginUrl = $"{config["AppSettings:ClientUrl"]}/Home/Login";

                string body = $@"
    <p>Hello <strong>{user.UsernameDisplay}</strong>,</p>
    <p>Your password has been <strong>reset by the system administrator</strong>.</p>
    <p>Please use this temporary password to log in, then change it immediately:</p>

    <div style='margin:20px 0; text-align:center;'>
        <span style='display:inline-block; background:#0b6efd; color:#fff; padding:10px 20px;
            border-radius:6px; font-weight:600; letter-spacing:1px;'>{temporaryPassword}</span>
    </div>

    <p>Click the button below to go to the login page:</p>
    <div style='text-align:center; margin:25px 0;'>
        <a href='{loginUrl}' style='background:linear-gradient(135deg,#0b6efd,#2563eb);
            color:white; text-decoration:none; padding:12px 24px; border-radius:6px;
            font-weight:600;'>Go to Login</a>
    </div>

    <p>If you did not request this reset, please contact your administrator immediately.</p>";

                string html = EmailTemplateHelper.BuildTemplate(logoUrl, "Password Reset by Administrator", body);
                await emailSender.SendEmailAsync(email, "Kovsie Assets Tracker Password Reset", html);

                return Ok(ApiResponseDTO<string>.SuccessResponse(null, $"Password for {email} reset successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDTO<string>.FailResponse("Failed to reset password.", ex.Message));
            }
        }


        /// <summary>Delete a user (Admin only).</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            // Delete associated profile image
            if (!string.IsNullOrEmpty(user.ProfileImageFileName) && user.ProfileImageFileName != defaultImage)
            {
                var filePath = Path.Combine(env.WebRootPath, "images", folderName, user.ProfileImageFileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Failed to delete user.", result.Errors.FirstOrDefault()?.Description));

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "User deleted successfully."));
        }
    }
}
