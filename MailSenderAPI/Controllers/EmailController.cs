
using MailSenderAPI.Models;
using MailSenderAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace MailSenderAPI.Controllers
{
    [Route("api/email")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

   
        [HttpPost("sendMail")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest emailRequest)
        {
               var response = await _emailService.SendEmailAsync(emailRequest.To, emailRequest.Subject, emailRequest.Body);
                return Ok(response);
        }
    }
}
