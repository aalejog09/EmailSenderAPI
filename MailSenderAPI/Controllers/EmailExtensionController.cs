using MailSenderAPI.Models;
using MailSenderAPI.Models.NewFolder;
using MailSenderAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;

namespace MailSenderAPI.Controllers
{
    [Route("api/email/extension/")]
    [ApiController]
    public class EmailExtensionController : ControllerBase
    {
        private readonly EmailExtensionService _emailExtensionService;

        public EmailExtensionController(EmailExtensionService emailExtensionService)
        {
            _emailExtensionService = emailExtensionService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddExtension([FromBody] ExtensionRqDTO extension)
        {
                var result = await _emailExtensionService.AddExtensionAsync(extension.ExtensionName);
                return Ok(result);
        
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllExtensions()
        {
                var result = await _emailExtensionService.GetAllExtensionsAsync();
                return Ok(result);

           
        }

        [HttpGet("find/{extension}")]
        public async Task<IActionResult> GetByExtension([FromRoute] string extension)
        {
                var result = await _emailExtensionService.GetByExtensionAsync(extension);
                return Ok(result);
            
        }

        [HttpPut("change-status")]
        public async Task<IActionResult> ChangeStatus([FromQuery] string extension, [FromQuery] bool status)
        {
                var result = await _emailExtensionService.ChangeExtensionStatus(extension, status);
                return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteExtension([FromQuery] string extension)
        {
                var response = await _emailExtensionService.DeleteExtensionAsync(extension);
                return Ok(response);
        }
    }
}