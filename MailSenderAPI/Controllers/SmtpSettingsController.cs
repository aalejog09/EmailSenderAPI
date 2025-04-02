using MailSenderAPI.Models;
using MailSenderAPI.Models.DTO;
using MailSenderAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MailSenderAPI.Controllers
{
    [Route("api/email/settings")]
    [ApiController]
    public class SmtpSettingsController : ControllerBase
    {
        private readonly SmtpSettingsService _smtpSettingsService;

        public SmtpSettingsController(SmtpSettingsService smtpSettingsService)
        {
            _smtpSettingsService = smtpSettingsService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetSmtpSettings()
        {
            var response = await _smtpSettingsService.GetAllSmtpSettingsAsync();
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] SmtpSettignsRqDTO smtpSettings)
        {
            var response = await _smtpSettingsService.CreateAsync(smtpSettings);
            return Ok(response);
        }

        [HttpGet("{FromEmail}")]
        public async Task<IActionResult> GetById(string FromEmail)
        {
            var response = await _smtpSettingsService.GetSettingsDTOByEmailAsync(FromEmail);
            return Ok(response);

        }

        [HttpDelete("delete/{FromEmail}")]
        public async Task<IActionResult> Delete(string FromEmail)
        {
            var response = await _smtpSettingsService.DeleteAsync(FromEmail);
            return Ok(response);
        }
    }
}
