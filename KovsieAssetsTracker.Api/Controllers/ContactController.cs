using KovsieAssetsTracker.Application.DTOs.ContactDTOs;
using KovsieAssetsTracker.Application.DTOs.SharedDTOs;
using KovsieAssetsTracker.Application.Helpers;
using KovsieAssetsTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace KovsieAssetsTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public ContactController(IEmailSender emailSender, IConfiguration config)
        {
            _emailSender = emailSender;
            _config = config;
        }

        [HttpPost("Send")]
        public async Task<IActionResult> Send([FromBody] ContactRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseDTO<string>.FailResponse("Invalid request data."));
            }

            try
            {
                string logoUrl = $"{Request.Scheme}://{Request.Host}/images/icon_logo_transparent.png";

                string body = $@"
                    <p><strong>Name:</strong> {request.Name}</p>
                    <p><strong>Email:</strong> {request.Email}</p>
                    <p><strong>Message:</strong><br/>{request.Message}</p>";

                string htmlTemplate = EmailTemplateHelper.BuildTemplate(
                    logoUrl: logoUrl,
                    title: "New Contact Form Submission",
                    body: body
                );

                await _emailSender.SendEmailAsync(
                    toEmail: "nxumalosimphiweterrence@gmail.com", 
                    subject: $"New Message from {request.Name} ({request.Email})",
                    message: htmlTemplate
                );

                return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Your message was sent successfully!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDTO<string>.FailResponse(
                    $"Failed to send email. {ex.Message}",
                    "Email sending failed."
                ));
            }
        }
    }
}
