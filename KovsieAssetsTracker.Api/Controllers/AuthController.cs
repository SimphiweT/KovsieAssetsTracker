using KovsieAssetsTracker.Application.DTOs;
using KovsieAssetsTracker.Application.DTOs.AuthDTOs;
using KovsieAssetsTracker.Application.DTOs.SharedDTOs;
using KovsieAssetsTracker.Application.DTOs.UserDTO;
using KovsieAssetsTracker.Application.Helpers;
using KovsieAssetsTracker.Application.Services;
using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KovsieAssetsTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration config;
        private readonly IRepositoryWrapper repo;
        private readonly IWebHostEnvironment env;
        private readonly IEmailSender emailSender;

        private static readonly string folderName = "users";
        private static readonly string defaultImage = "default-user.png";
        private static readonly string accessFormFolder = "access_forms";

        public AuthController(
            UserManager<ApplicationUser> _userManager,
            SignInManager<ApplicationUser> _signInManager,
            IConfiguration _config,
            IRepositoryWrapper _repo,
            IWebHostEnvironment _env,
            IEmailSender _emailSender)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            config = _config;
            repo = _repo;
            env = _env;
            emailSender = _emailSender;
        }

        // -------------------- Helpers --------------------
        private string GetBaseUrl(string folder) =>
            $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/{folder}/";

        private string GetAccessFormBaseUrl() =>
            $"{Request.Scheme}://{Request.Host}{Request.PathBase}/files/{accessFormFolder}/";

        private string GetLogoUrl() =>
            $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/icon_logo_transparent.png";

        private IEnumerable<string> GetAdminNotifyEmails()
        {
            var csv = config["AppSettings:AdminNotifyEmails"] ?? "";
            return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

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

        private async Task<string?> SaveAccessFormAsync(IFormFile? file)
        {
            if (file is { Length: > 0 })
            {
                var folder = Path.Combine(env.WebRootPath, "files", accessFormFolder);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var path = Path.Combine(folder, fileName);

                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);
                return fileName;
            }

            return null;
        }

        // -------------------- Register --------------------
        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> Register([FromForm] CreateUserRequestDTO dto)
        {
            if (await userManager.FindByEmailAsync(dto.UserEmail) != null)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Email already taken."));

            if (await userManager.Users.AnyAsync(u => u.UsernameDisplay == dto.UsernameDisplay))
                return BadRequest(ApiResponseDTO<string>.FailResponse("Display name already taken."));

            if (await userManager.Users.AnyAsync(u => u.StudentStaffNumber == dto.StudentStaffNumber))
                return BadRequest(ApiResponseDTO<string>.FailResponse("Staff/Student number already registered."));

            var profileImageFileName = await SaveImageAsync(dto.ImageFile);

            var user = new ApplicationUser
            {
                UserName = dto.UserEmail,
                Email = dto.UserEmail,
                UsernameDisplay = dto.UsernameDisplay,
                StudentStaffNumber = dto.StudentStaffNumber,
                ProfileImageFileName = profileImageFileName,
                ContactNumber = dto.ContactNumber,
                Description = dto.Description,
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(ApiResponseDTO<string>.FailResponse(errors, "Failed to create user."));
            }

            await userManager.AddToRoleAsync(user, dto.UserRole ?? ConstantsUtils.STAFF_ROLE);

            var response = new UserResponseDTO
            {
                UserId = user.Id,
                UsernameDisplay = user.UsernameDisplay,
                UserEmail = user.Email,
                StudentStaffNumber = user.StudentStaffNumber,
                ProfileImageFileName = GetBaseUrl(folderName) + user.ProfileImageFileName,
                ContactNumber = user.ContactNumber,
                Description = user.Description,
                UserRole = (await userManager.GetRolesAsync(user)).FirstOrDefault(),
                CreatedDate = user.CreatedDate
            };

            return Ok(ApiResponseDTO<UserResponseDTO>.SuccessResponse(response, "User created successfully."));
        }

        // -------------------- Login --------------------
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(ApiResponseDTO<string>.FailResponse("Invalid email or password."));

            var roles = await userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            var profileUrl = GetBaseUrl(folderName) + (string.IsNullOrEmpty(user.ProfileImageFileName) ? defaultImage : user.ProfileImageFileName);

            var response = new LoginResponseDTO
            {
                UserId = user.Id,
                UserDisplayName = user.UsernameDisplay ?? "Unknown User",
                UserEmail = user.Email ?? "N/A",
                UserRole = roles.FirstOrDefault() ?? ConstantsUtils.STAFF_ROLE,
                ProfilePictureUrl = profileUrl,
                Token = token,
                TokenExpirationTime = DateTime.UtcNow.AddHours(3)
            };

            return Ok(ApiResponseDTO<LoginResponseDTO>.SuccessResponse(response, "Login successful."));
        }
        // ======================= GET ALL ACCESS REQUESTS =======================
        [HttpGet("access-requests")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> GetAllAccessRequests()
        {
            try
            {
                var requests = await repo.UserAccessRequest.GetAllAsync();

                var filesBase = GetAccessFormBaseUrl();
                var responses = requests.Select(r => new AccessResponseDTO
                {
                    Id = r.Id,
                    FullName = r.DisplayName,
                    Email = r.Email,
                    RequestedRole = r.RoleApplied,
                    DocumentPath = string.IsNullOrEmpty(r.PdfFilePath) ? "" : $"{filesBase}{r.PdfFilePath}",
                    Status = r.UpdatedAt == null ? "Pending" : (r.IsApproved ? "Approved" : "Declined"),
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.UpdatedAt
                }).OrderByDescending(r => r.CreatedAt).ToList();

                return Ok(ApiResponseDTO<IEnumerable<AccessResponseDTO>>.SuccessResponse(responses, "Access requests retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDTO<string>.FailResponse(ex.Message, "Failed to load access requests."));
            }
        }


        // -------------------- Request Access --------------------
        [HttpPost("request-access")]
        [Consumes("multipart/form-data")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestAccess([FromForm] AccessRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Invalid request data."));

            var exists = await repo.UserAccessRequest
                .GetAllByCondition(x => x.Email == dto.Email || x.DisplayName == dto.DisplayName);

            if (exists.Any())
                return BadRequest(ApiResponseDTO<string>.FailResponse("A request with the same email or name already exists."));

            var pdfFileName = await SaveAccessFormAsync(dto.PdfDocument);

            var request = new UserAccessRequest
            {
                DisplayName = dto.DisplayName,
                Email = dto.Email,
                RoleApplied = dto.RoleApplied,
                Message = dto.Message,
                PdfFilePath = pdfFileName,
                CreatedAt = DateTime.UtcNow,
                IsApproved = false
            };

            await repo.UserAccessRequest.CreateAsync(request);
            await repo.SaveAsync();

            // --- Notify Admin ---
            try
            {
                var logoUrl = GetLogoUrl();
                var filesBase = GetAccessFormBaseUrl();
                var pdfLink = string.IsNullOrEmpty(pdfFileName) ? "(no document)" : $"{filesBase}{pdfFileName}";

                var subject = "New Kovsie Assets Access Request";
                var body = $@"
                    <p><strong>New access request received</strong></p>
                    <p>
                        <strong>Name:</strong> {request.DisplayName}<br/>
                        <strong>Email:</strong> {request.Email}<br/>
                        <strong>Role Requested:</strong> {request.RoleApplied}<br/>
                        <strong>Message:</strong> {(string.IsNullOrWhiteSpace(request.Message) ? "(none)" : request.Message)}<br/>
                        <strong>Document:</strong> <a href='{pdfLink}'>Download</a><br/>
                        <strong>Submitted:</strong> {request.CreatedAt:dd MMM yyyy HH:mm (UTC)}
                    </p>
                    <p style='margin-top:16px'>
                        You can review this request in the Admin portal.
                    </p>";

                var html = EmailTemplateHelper.BuildTemplate(logoUrl, "New Access Request", body);

                foreach (var adminEmail in GetAdminNotifyEmails())
                {
                    await emailSender.SendEmailAsync(adminEmail, subject, html);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] Admin notification failed: {ex.Message}");
            }

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Access request submitted successfully."));
        }

        [HttpGet("access-requests/pending")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> GetPendingAccessRequests()
        {
            var result = await repo.UserAccessRequest.GetPendingRequestsAsync();
            var filesBase = GetAccessFormBaseUrl();

            var mapped = result.Select(r => new AccessResponseDTO
            {
                Id = r.Id,
                FullName = r.DisplayName,
                Email = r.Email,
                RequestedRole = r.RoleApplied,
                DocumentPath = string.IsNullOrEmpty(r.PdfFilePath) ? "" : $"{filesBase}{r.PdfFilePath}",
                Status = "Pending",
                CreatedAt = r.CreatedAt
            });

            return Ok(ApiResponseDTO<IEnumerable<AccessResponseDTO>>.SuccessResponse(mapped, "Pending access requests."));
        }

        [HttpGet("access-requests/reviewed")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> GetReviewedAccessRequests()
        {
            var result = await repo.UserAccessRequest.GetReviewedRequestsAsync();
            var filesBase = GetAccessFormBaseUrl();

            var mapped = result.Select(r => new AccessResponseDTO
            {
                Id = r.Id,
                FullName = r.DisplayName,
                Email = r.Email,
                RequestedRole = r.RoleApplied,
                DocumentPath = string.IsNullOrEmpty(r.PdfFilePath) ? "" : $"{filesBase}{r.PdfFilePath}",
                Status = r.IsApproved ? "Approved" : "Declined",
                CreatedAt = r.CreatedAt,
                ReviewedAt = r.UpdatedAt
            });

            return Ok(ApiResponseDTO<IEnumerable<AccessResponseDTO>>.SuccessResponse(mapped, "Reviewed access requests."));
        }


        // -------------------- Approve Access --------------------
        [HttpPut("approve-access/{id}")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> ApproveAccess(int id)
        {
            var request = await repo.UserAccessRequest.GetByIdAsync(id);
            if (request == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Access request not found."));

            request.IsApproved = true;
            request.ApprovedBy = User.Identity?.Name ?? "System";
            request.UpdatedAt = DateTime.UtcNow;

            await repo.UserAccessRequest.UpdateAsync(request);
            await repo.SaveAsync();

            // Email User
            try
            {
                string logoUrl = GetLogoUrl();
                string body = $@"
                    <p>Dear <strong>{request.DisplayName}</strong>,</p>
                    <p>Your request to access the <strong>Kovsie Assets Tracker</strong> system has been 
                    <strong style='color:green;'>approved</strong>.</p>
                    <p>You will be contacted soon to finalize your account setup.</p>
                    <hr/>
                    <p><small>Approved by: {request.ApprovedBy}</small><br/>
                    <small>Date: {DateTime.UtcNow:dd MMM yyyy HH:mm}</small></p>";

                string html = EmailTemplateHelper.BuildTemplate(logoUrl, "Access Request Approved", body);
                await emailSender.SendEmailAsync(request.Email, "Kovsie Assets Tracker Access Approved", html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] Approval mail failed: {ex.Message}");
            }

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Access request approved and email sent."));
        }

        // -------------------- Decline Access --------------------
        [HttpPut("decline/{id}")]
        [Authorize(Roles = ConstantsUtils.ADMIN_ROLE)]
        public async Task<IActionResult> DeclineAccess(int id, [FromBody] string? reason)
        {
            var request = await repo.UserAccessRequest.GetByIdAsync(id);
            if (request == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Access request not found."));

            request.IsApproved = false;
            request.DeclinedBy = User.Identity?.Name ?? "System";
            request.UpdatedAt = DateTime.UtcNow;

            await repo.UserAccessRequest.UpdateAsync(request);
            await repo.SaveAsync();

            try
            {
                string logoUrl = GetLogoUrl();
                string reasonHtml = !string.IsNullOrWhiteSpace(reason)
                    ? $"<p><strong>Reason:</strong> {reason}</p>"
                    : "";

                string body = $@"
                    <p>Dear <strong>{request.DisplayName}</strong>,</p>
                    <p>Your request to access the <strong>Kovsie Assets Tracker</strong> system has been 
                    <strong style='color:red;'>declined</strong>.</p>
                    {reasonHtml}
                    <p>You may reapply later or contact your administrator for clarification.</p>
                    <hr/>
                    <p><small>Reviewed by: {request.DeclinedBy}</small><br/>
                    <small>Date: {DateTime.UtcNow:dd MMM yyyy HH:mm}</small></p>";

                string html = EmailTemplateHelper.BuildTemplate(logoUrl, "Access Request Declined", body);
                await emailSender.SendEmailAsync(request.Email, "❌ Kovsie Assets Tracker Access Declined", html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] Decline mail failed: {ex.Message}");
            }

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Access request declined and email sent."));
        }
        /// <summary> ****** Change Password ****** </summary>

        [HttpPost("change-password")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.AGENT_ROLE},{ConstantsUtils.STAFF_ROLE}")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO dto)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = null,
                    Error = "You are not authorized to perfom this action.",
                    Data = null
                });

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = "Failed to change password",
                    Error = result.Errors.ToString(),
                    Data = null
                });

            return Ok(new ApiResponseDTO<string>
            {
                Success = true,
                Message = "Password changed successfully...",
                Error = null,
                Data = null
            });
        }

        /// <summary> ****** Forgot Password (Send Token) ****** </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO dto)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    // Don't reveal user existence
                    return Ok(ApiResponseDTO<string>.SuccessResponse(
                        null,
                        "If an account exists, a reset link has been sent."
                    ));
                }

                // Generate reset token
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = Uri.EscapeDataString(token);

                // Use PasswordResetUrl from config
                var resetBaseUrl = config["AppSettings:PasswordResetUrl"] ?? $"{config["AppSettings:BaseUrl"]}/reset-password";
                var resetLink = $"{resetBaseUrl}?email={dto.Email}&token={encodedToken}";

                // Build Kovsie-branded HTML email
                string logoUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/icon_logo_transparent.png";
                string bodyHtml = $@"
            <p>Hello <strong>{user.UsernameDisplay}</strong>,</p>
            <p>We received a request to reset your password for your <strong>Kovsie Assets Tracker</strong> account.</p>
            <p>Click the button below to reset your password securely:</p>

            <div style='text-align:center; margin:25px 0;'>
                <a href='{resetLink}' style='background:linear-gradient(135deg,#0b6efd,#2563eb);
                    color:white; text-decoration:none; padding:12px 24px; border-radius:6px;
                    font-weight:600; display:inline-block;'>🔐 Reset My Password</a>
            </div>

            <p>If the button above doesn’t work, copy and paste this link into your browser:</p>
            <p style='word-break:break-all; color:#0b6efd;'>{resetLink}</p>

            <p>If you did not request a password reset, please ignore this email — your account remains secure.</p>";

                // Use your EmailTemplateHelper for consistent branding
                string htmlTemplate = EmailTemplateHelper.BuildTemplate(logoUrl, "Password Reset Request", bodyHtml);

                // Send email
                await emailSender.SendEmailAsync(dto.Email, " Kovsie Assets Password Reset", htmlTemplate);

                string sResponseMessage = $"If an account exists, a reset link has been sent to {dto.Email}.";

                return Ok(ApiResponseDTO<string>.SuccessResponse(null, sResponseMessage));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ForgotPassword] Error: {ex.Message}");
                return StatusCode(500, ApiResponseDTO<string>.FailResponse("Failed to send reset email.", ex.Message));
            }
        }



        /// <summary> ****** Reset Password With Token ****** </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = "Invalid Request",
                    Error = null,
                    Data = null
                });

            var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new ApiResponseDTO<string>
                {
                    Success = false,
                    Message = "Reset password failed.",
                    Error = result.Errors.ToString(),
                    Data = null
                });


            string sResponseMessage = $"Password reset successful...\nToken: {dto.Token}\nEmail: {dto.Email}";
            return Ok(new ApiResponseDTO<string>
            {
                Success = true,
                Message = sResponseMessage, //For Testing
                Error = null,
                Data = null
            });
        }






















        // -------------------- JWT Token --------------------
        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new("UsernameDisplay", user.UsernameDisplay ?? ""),
                new("StudentStaffNumber", user.StudentStaffNumber ?? ""),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(3);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
